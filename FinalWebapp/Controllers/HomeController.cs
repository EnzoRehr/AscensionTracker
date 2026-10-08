using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FinalWebapp.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using FinalWebapp.Data;
using System.Linq;

namespace FinalWebapp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Main Index page
        public IActionResult Index()
        {
            var model = new HomeViewModel
            {
                HasCompletedAssessment = false,
                ChestRank = null,
                BicepsRank = null
            };

            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null)
                {
                    var userId = int.Parse(userIdClaim.Value);
                    
                    const int CHEST_MUSCLE_GROUP_ID = 17;
                    const int BICEPS_MUSCLE_GROUP_ID = 18;

                    var chestRanking = _context.UserMuscleRankings
                        .FirstOrDefault(r => r.UserId == userId && r.MuscleGroupId == CHEST_MUSCLE_GROUP_ID);
                    
                    var bicepsRanking = _context.UserMuscleRankings
                        .FirstOrDefault(r => r.UserId == userId && r.MuscleGroupId == BICEPS_MUSCLE_GROUP_ID);

                    if (chestRanking != null && bicepsRanking != null)
                    {
                        model.HasCompletedAssessment = true;
                        model.ChestRank = chestRanking.Rank;
                        model.BicepsRank = bicepsRanking.Rank;
                    }

                    // Get active workout for widget
                    ViewBag.ActiveWorkout = GetActiveWorkout(userId);
                }
            }

            return View(model);
        }

        #region Achievements
        public IActionResult Achievements()
        {
            var model = new AchievementsViewModel();

            // Get all achievement categories with their achievements
            model.Categories = _context.AchievementCategories
                .Select(cat => new AchievementCategoryGroup
                {
                    CategoryId = cat.CategoryId,
                    Name = cat.Name,
                    Description = cat.Description,
                    Achievements = cat.Achievements.ToList()
                })
                .ToList();

            // If user is authenticated, get their achievements and total points
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null)
                {
                    var userId = int.Parse(userIdClaim.Value);

                    // Get user's claimed achievements
                    model.UserAchievements = _context.UserAchievements
                        .Where(ua => ua.UserId == userId)
                        .ToList();

                    // Get user's total points
                    var user = _context.Users.FirstOrDefault(u => u.Id == userId);
                    model.UserTotalPoints = user?.TotalPoints ?? 0;
                }
            }

            return View(model);
        }

        // POST: Claim achievement
        [HttpPost]
        public IActionResult ClaimAchievement([FromBody] ClaimAchievementRequest request)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "You must be logged in to claim achievements" });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            var userId = int.Parse(userIdClaim.Value);

            // Get the achievement
            var achievement = _context.Achievements.FirstOrDefault(a => a.AchievementId == request.AchievementId);
            if (achievement == null)
            {
                return Json(new { success = false, message = "Achievement not found" });
            }

            // Check if user already claimed this achievement recently (within cooldown)
            var existingClaim = _context.UserAchievements
                .Where(ua => ua.UserId == userId && ua.AchievementId == request.AchievementId)
                .OrderByDescending(ua => ua.DateEarned)
                .FirstOrDefault();

            if (existingClaim != null && existingClaim.DateEarned.AddSeconds(30) > DateTime.UtcNow)
            {
                var remainingSeconds = (int)(existingClaim.DateEarned.AddSeconds(30) - DateTime.UtcNow).TotalSeconds;
                return Json(new 
                { 
                    success = false, 
                    message = $"Cooldown active. Wait {remainingSeconds} seconds." 
                });
            }

            // Create new user achievement
            var userAchievement = new UserAchievement
            {
                UserId = userId,
                AchievementId = achievement.AchievementId,
                DateEarned = DateTime.UtcNow,
                PointsEarned = achievement.PointsAwarded,
                IsDisplayed = true
            };

            _context.UserAchievements.Add(userAchievement);

            // Update user's total points
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                user.TotalPoints += achievement.PointsAwarded;
            }

            _context.SaveChanges();

            return Json(new
            {
                success = true,
                achievementName = achievement.Name,
                pointsEarned = achievement.PointsAwarded,
                newTotalPoints = user?.TotalPoints ?? 0
            });
        }
        #endregion

        #region Workout System
        public IActionResult StartWorkout()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = new StartWorkoutViewModel
            {
                WorkoutPlans = _context.WorkoutPlans.ToList(),
                Exercises = _context.Exercises
                    .Include(e => e.MuscleGroup)
                    .Select(e => new ExerciseDto
                    {
                        ExerciseId = e.ExerciseId,
                        Name = e.Name,
                        MuscleGroupName = e.MuscleGroup.Name
                    })
                    .ToList()
            };

            return View(model);
        }

        // POST: Start Plan Workout
        [HttpPost]
        public IActionResult StartPlanWorkout(int WorkoutPlanId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            var userId = int.Parse(userIdClaim.Value);

            var workoutPlan = _context.WorkoutPlans.FirstOrDefault(wp => wp.WorkoutPlanId == WorkoutPlanId);
            if (workoutPlan == null)
            {
                TempData["ErrorMessage"] = "Workout plan not found.";
                return RedirectToAction("StartWorkout");
            }

            // Calculate points based on difficulty
            int points = workoutPlan.Difficulty switch
            {
                "Beginner" => 75,
                "Intermediate" => 100,
                "Advanced" => 125,
                _ => 75
            };

            // Create user workout
            var userWorkout = new UserWorkout
            {
                UserId = userId,
                WorkoutPlanId = WorkoutPlanId,
                StartTime = DateTime.UtcNow,
                EstimatedDurationMinutes = workoutPlan.EstimatedDurationMinutes,
                PointsEarned = points,
                IsCompleted = false
            };

            _context.UserWorkouts.Add(userWorkout);
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Started {workoutPlan.Name}! Complete it to earn {points} points.";
            return RedirectToAction("Index");
        }

        // POST: Start Custom Workout
        [HttpPost]
        public IActionResult StartCustomWorkout(List<CustomWorkoutEntry> Exercises)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            var userId = int.Parse(userIdClaim.Value);

            if (Exercises == null || !Exercises.Any())
            {
                TempData["ErrorMessage"] = "Please add at least one exercise.";
                return RedirectToAction("StartWorkout");
            }

            // Calculate total points and duration
            int totalPoints = 0;
            int totalSeconds = 0;

            foreach (var exercise in Exercises)
            {
                int reps = exercise.Sets * exercise.Reps;
                totalPoints += reps * 8;  // 8 points per rep
                totalSeconds += reps * 5;  // 5 seconds per rep
            }

            int totalMinutes = (int)Math.Ceiling(totalSeconds / 60.0);

            // Create user workout for first exercise
            var firstExercise = Exercises.First();
            var userWorkout = new UserWorkout
            {
                UserId = userId,
                ExerciseId = firstExercise.ExerciseId,
                Sets = firstExercise.Sets,
                Reps = firstExercise.Reps,
                StartTime = DateTime.UtcNow,
                EstimatedDurationMinutes = totalMinutes,
                PointsEarned = totalPoints,
                IsCompleted = false
            };

            _context.UserWorkouts.Add(userWorkout);

            // Add remaining exercises as separate workouts
            for (int i = 1; i < Exercises.Count; i++)
            {
                var ex = Exercises[i];
                int exPoints = ex.Sets * ex.Reps * 8;
                int exMinutes = (int)Math.Ceiling((ex.Sets * ex.Reps * 5) / 60.0);

                _context.UserWorkouts.Add(new UserWorkout
                {
                    UserId = userId,
                    ExerciseId = ex.ExerciseId,
                    Sets = ex.Sets,
                    Reps = ex.Reps,
                    StartTime = DateTime.UtcNow,
                    EstimatedDurationMinutes = exMinutes,
                    PointsEarned = exPoints,
                    IsCompleted = false
                });
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Started custom workout! Complete it to earn {totalPoints} points.";
            return RedirectToAction("Index");
        }

        // POST: Claim Workout
        [HttpPost]
        public IActionResult ClaimWorkout([FromBody] ClaimWorkoutRequest request)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "You must be logged in" });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Json(new { success = false, message = "User not found" });
            }
            var userId = int.Parse(userIdClaim.Value);

            var workout = _context.UserWorkouts
                .Include(w => w.WorkoutPlan)
                .Include(w => w.Exercise)
                .FirstOrDefault(w => w.UserWorkoutId == request.UserWorkoutId && w.UserId == userId);

            if (workout == null)
            {
                return Json(new { success = false, message = "Workout not found" });
            }

            if (workout.IsCompleted)
            {
                return Json(new { success = false, message = "Workout already claimed" });
            }

            // Check if enough time has passed
            var elapsedMinutes = (DateTime.UtcNow - workout.StartTime).TotalMinutes;
            if (elapsedMinutes < workout.EstimatedDurationMinutes)
            {
                var remaining = (int)Math.Ceiling(workout.EstimatedDurationMinutes - elapsedMinutes);
                return Json(new { success = false, message = $"Please wait {remaining} more minutes" });
            }

            // Mark workout as completed
            workout.IsCompleted = true;
            workout.EndTime = DateTime.UtcNow;

            // Update user points and workout count
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                user.TotalPoints += workout.PointsEarned;
                user.WorkoutCount += 1;
            }

            _context.SaveChanges();

            return Json(new
            {
                success = true,
                pointsEarned = workout.PointsEarned,
                newTotalPoints = user?.TotalPoints ?? 0,
                workoutCount = user?.WorkoutCount ?? 0
            });
        }

        // Helper method to get active workout for widget
        private UserWorkout GetActiveWorkout(int userId)
        {
            return _context.UserWorkouts
                .Include(w => w.WorkoutPlan)
                .Include(w => w.Exercise)
                .ThenInclude(e => e.MuscleGroup)
                .Where(w => w.UserId == userId && !w.IsCompleted)
                .OrderByDescending(w => w.StartTime)
                .FirstOrDefault();
        }
        #endregion

        #region Get Started Assessment
        public IActionResult GetStarted()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitGetStarted(decimal BenchPressWeight, decimal BicepsCurlWeight)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            const int CHEST_MUSCLE_GROUP_ID = 17;
            const int BICEPS_MUSCLE_GROUP_ID = 18;

            try
            {
                var benchRank = CalculateBenchPressRank(BenchPressWeight);
                
                var existingChestRank = await _context.UserMuscleRankings
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.MuscleGroupId == CHEST_MUSCLE_GROUP_ID);

                if (existingChestRank != null)
                {
                    existingChestRank.Rank = benchRank;
                    existingChestRank.MaxWeight = BenchPressWeight;
                    existingChestRank.LastUpdated = DateTime.UtcNow;
                    existingChestRank.RankDate = DateTime.UtcNow;
                }
                else
                {
                    _context.UserMuscleRankings.Add(new UserMuscleRanking
                    {
                        UserId = userId,
                        MuscleGroupId = CHEST_MUSCLE_GROUP_ID,
                        Rank = benchRank,
                        MaxWeight = BenchPressWeight,
                        LastUpdated = DateTime.UtcNow,
                        RankDate = DateTime.UtcNow
                    });
                }

                var bicepsRank = CalculateBicepsCurlRank(BicepsCurlWeight);
                
                var existingBicepsRank = await _context.UserMuscleRankings
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.MuscleGroupId == BICEPS_MUSCLE_GROUP_ID);

                if (existingBicepsRank != null)
                {
                    existingBicepsRank.Rank = bicepsRank;
                    existingBicepsRank.MaxWeight = BicepsCurlWeight;
                    existingBicepsRank.LastUpdated = DateTime.UtcNow;
                    existingBicepsRank.RankDate = DateTime.UtcNow;
                }
                else
                {
                    _context.UserMuscleRankings.Add(new UserMuscleRanking
                    {
                        UserId = userId,
                        MuscleGroupId = BICEPS_MUSCLE_GROUP_ID,
                        Rank = bicepsRank,
                        MaxWeight = BicepsCurlWeight,
                        LastUpdated = DateTime.UtcNow,
                        RankDate = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"🎉 Assessment complete! Chest: {GetRankEmoji(benchRank)} {benchRank}, Biceps: {GetRankEmoji(bicepsRank)} {bicepsRank}";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while saving your assessment.";
                return RedirectToAction("GetStarted");
            }
        }

        private string CalculateBenchPressRank(decimal weight)
        {
            if (weight < 20) return "Wood";
            if (weight < 40) return "Bronze";
            if (weight < 60) return "Silver";
            if (weight < 80) return "Gold";
            if (weight < 100) return "Platinum";
            if (weight < 120) return "Emerald";
            if (weight < 150) return "Diamond";
            return "Champion";
        }

        private string CalculateBicepsCurlRank(decimal weight)
        {
            if (weight < 8) return "Wood";
            if (weight < 15) return "Bronze";
            if (weight < 22) return "Silver";
            if (weight < 30) return "Gold";
            if (weight < 38) return "Platinum";
            if (weight < 46) return "Emerald";
            if (weight < 55) return "Diamond";
            return "Champion";
        }

        private string GetRankEmoji(string rank)
        {
            return rank switch
            {
                "Wood" => "🪵",
                "Bronze" => "🥉",
                "Silver" => "🥈",
                "Gold" => "🥇",
                "Platinum" => "💎",
                "Emerald" => "💚",
                "Diamond" => "💠",
                "Champion" => "👑",
                _ => "🏅"
            };
        }
        #endregion

        #region Muscle Map & Rankings
        public IActionResult MuscleMap()
        {
            var model = new MuscleMapViewModel();

            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null)
                {
                    var userId = int.Parse(userIdClaim.Value);

                    model.Rankings = _context.UserMuscleRankings
                        .Where(r => r.UserId == userId)
                        .Select(r => new MuscleRankingDto
                        {
                            MuscleGroupId = r.MuscleGroupId,
                            MuscleName = r.MuscleGroup.Name,
                            Rank = r.Rank,
                            MaxWeight = r.MaxWeight,
                            LastUpdated = r.LastUpdated
                        })
                        .ToList();
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult GetUserMuscleRankings()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new List<object>());
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Json(new List<object>());
            }
            var userId = int.Parse(userIdClaim.Value);

            var rankings = _context.UserMuscleRankings
                .Where(r => r.UserId == userId)
                .Select(r => new
                {
                    muscleGroupId = r.MuscleGroupId,
                    muscleName = r.MuscleGroup.Name,
                    rank = r.Rank,
                    maxWeight = r.MaxWeight,
                    lastUpdated = r.LastUpdated
                })
                .ToList();

            return Json(rankings);
        }

        public IActionResult GlobalRanking()
        {
            var model = new LeaderboardViewModel();

            // Get top 10 users by total points
            model.TopUsers = _context.Users
                .OrderByDescending(u => u.TotalPoints)
                .ThenByDescending(u => u.WorkoutCount)
                .Take(10)
                .Select(u => new LeaderboardUserDto
                {
                    Username = u.Username,
                    TotalPoints = u.TotalPoints,
                    WorkoutCount = u.WorkoutCount,
                    DateJoined = u.DateJoined
                })
                .ToList();

            // Get total user count
            model.TotalUsers = _context.Users.Count();

            // If user is authenticated, get their ranking
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null)
                {
                    var userId = int.Parse(userIdClaim.Value);
                    var currentUser = _context.Users.FirstOrDefault(u => u.Id == userId);

                    if (currentUser != null)
                    {
                        // Calculate user's rank
                        var rank = _context.Users
                            .Count(u => u.TotalPoints > currentUser.TotalPoints || 
                                        (u.TotalPoints == currentUser.TotalPoints && u.WorkoutCount > currentUser.WorkoutCount)) + 1;

                        model.CurrentUserRanking = new LeaderboardUserDto
                        {
                            Rank = rank,
                            Username = currentUser.Username,
                            TotalPoints = currentUser.TotalPoints,
                            WorkoutCount = currentUser.WorkoutCount,
                            DateJoined = currentUser.DateJoined
                        };
                    }
                }
            }

            return View(model);
        }

        public IActionResult MuscleGroupRanking()
        {
            return View();
        }
        #endregion

        #region Other Pages
        public IActionResult YTpage()
        {
            return View();
        }

        public IActionResult GymsCloseBy()
        {
            return View();
        }

        public IActionResult test()
        {
            return View();
        }

        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult DashBoard()
        {
            return View();
        }

        public IActionResult Edituser()
        {
            return View();
        }

        public IActionResult Users()
        {
            return View();
        }
        #endregion

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region Request Models
        public class ClaimAchievementRequest
        {
            public int AchievementId { get; set; }
        }

        public class ClaimWorkoutRequest
        {
            public int UserWorkoutId { get; set; }
        }
        #endregion
    }
}