using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinalWebapp.Models;
using FinalWebapp.Data;
using System.Security.Claims;

namespace FinalWebapp.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Admin Dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Auth");

            var stats = new
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalExercises = await _context.Exercises.CountAsync(),
                TotalWorkouts = await _context.UserWorkouts.CountAsync(),
                TotalAchievements = await _context.Achievements.CountAsync(),
                TotalMuscleGroups = await _context.MuscleGroups.CountAsync(),
                TotalWorkoutPlans = await _context.WorkoutPlans.CountAsync() // ADDED
            };

            ViewBag.Stats = stats;
            
            // For dashboard card display
            ViewBag.UsersCount = stats.TotalUsers;
            ViewBag.ExercisesCount = stats.TotalExercises;
            ViewBag.MuscleGroupsCount = stats.TotalMuscleGroups;
            ViewBag.AchievementsCount = stats.TotalAchievements;
            ViewBag.WorkoutPlansCount = stats.TotalWorkoutPlans; // ADDED
            
            return View();
        }

        #region Users Management
        public async Task<IActionResult> Users()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        public IActionResult CreateUser()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(User user)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            // Note: In production, you should hash the password here
            // For now, assuming password is already hashed or will be hashed in your auth system
            user.DateJoined = DateTime.UtcNow;
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "User created successfully!";
            return RedirectToAction("Users");
        }

        public async Task<IActionResult> EditUser(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(User user)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser == null) return NotFound();

            existingUser.Username = user.Username;
            existingUser.Email = user.Email;
            existingUser.TotalPoints = user.TotalPoints;
            existingUser.WorkoutCount = user.WorkoutCount;
            existingUser.ProfilePicture = user.ProfilePicture;
            
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "User updated successfully!";
            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "User deleted successfully!";
            }
            return RedirectToAction("Users");
        }
        #endregion

        #region Exercises Management
        public async Task<IActionResult> Exercises()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            var exercises = await _context.Exercises
                .Include(e => e.MuscleGroup)
                .Include(e => e.UserWorkouts) // ADDED for workout count
                .ToListAsync();
            return View(exercises);
        }

        public async Task<IActionResult> CreateExercise()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            ViewBag.MuscleGroups = await _context.MuscleGroups.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateExercise(Exercise exercise)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();
            
            // Get the muscle group name and updated count
            var muscleGroup = await _context.MuscleGroups
                .Include(mg => mg.Exercises)
                .FirstOrDefaultAsync(mg => mg.Id == exercise.MuscleGroupId);
            
            TempData["SuccessMessage"] = $"Exercise created successfully! {muscleGroup.Name} now has {muscleGroup.Exercises.Count} exercise(s).";
            return RedirectToAction("Exercises");
        }

        public async Task<IActionResult> EditExercise(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            var exercise = await _context.Exercises
                .Include(e => e.UserWorkouts)
                .FirstOrDefaultAsync(e => e.ExerciseId == id);
            if (exercise == null) return NotFound();
            ViewBag.MuscleGroups = await _context.MuscleGroups.ToListAsync();
            return View(exercise);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditExercise(Exercise exercise)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            _context.Exercises.Update(exercise);
            await _context.SaveChangesAsync();
            
            // Get the muscle group name and updated count
            var muscleGroup = await _context.MuscleGroups
                .Include(mg => mg.Exercises)
                .FirstOrDefaultAsync(mg => mg.Id == exercise.MuscleGroupId);
            
            TempData["SuccessMessage"] = $"Exercise updated successfully! {muscleGroup.Name} now has {muscleGroup.Exercises.Count} exercise(s).";
            return RedirectToAction("Exercises");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteExercise(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            var exercise = await _context.Exercises
                .Include(e => e.MuscleGroup)
                .FirstOrDefaultAsync(e => e.ExerciseId == id);
            
            if (exercise != null)
            {
                var muscleGroupId = exercise.MuscleGroupId;
                var muscleGroupName = exercise.MuscleGroup.Name;
                
                _context.Exercises.Remove(exercise);
                await _context.SaveChangesAsync();
                
                // Get updated count
                var updatedCount = await _context.Exercises.CountAsync(e => e.MuscleGroupId == muscleGroupId);
                
                TempData["SuccessMessage"] = $"Exercise deleted successfully! {muscleGroupName} now has {updatedCount} exercise(s).";
            }
            return RedirectToAction("Exercises");
        }
        #endregion

        #region MuscleGroups Management
        public async Task<IActionResult> MuscleGroups()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            var muscleGroups = await _context.MuscleGroups
                .Include(mg => mg.Exercises)
                .ToListAsync();
            return View(muscleGroups);
        }

        public IActionResult CreateMuscleGroup()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMuscleGroup(MuscleGroup muscleGroup)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            _context.MuscleGroups.Add(muscleGroup);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Muscle Group created successfully!";
            return RedirectToAction("MuscleGroups");
        }

        public async Task<IActionResult> EditMuscleGroup(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            var muscleGroup = await _context.MuscleGroups
                .Include(mg => mg.Exercises)
                .FirstOrDefaultAsync(mg => mg.Id == id);
            if (muscleGroup == null) return NotFound();
            return View(muscleGroup);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMuscleGroup(MuscleGroup muscleGroup)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            _context.MuscleGroups.Update(muscleGroup);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Muscle Group updated successfully!";
            return RedirectToAction("MuscleGroups");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMuscleGroup(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            var muscleGroup = await _context.MuscleGroups.FindAsync(id);
            if (muscleGroup != null)
            {
                _context.MuscleGroups.Remove(muscleGroup);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Muscle Group deleted successfully!";
            }
            return RedirectToAction("MuscleGroups");
        }
        #endregion

        #region Achievements Management
        public async Task<IActionResult> Achievements()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            var achievements = await _context.Achievements
                .Include(a => a.Category)
                .Include(a => a.UserAchievements) // ADDED for earned count
                .ToListAsync();
            return View(achievements);
        }

        public async Task<IActionResult> CreateAchievement()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            ViewBag.Categories = await _context.AchievementCategories.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAchievement(Achievement achievement)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            _context.Achievements.Add(achievement);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Achievement created successfully!";
            return RedirectToAction("Achievements");
        }

        public async Task<IActionResult> EditAchievement(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            var achievement = await _context.Achievements
                .Include(a => a.UserAchievements) // ADDED for earned count
                .FirstOrDefaultAsync(a => a.AchievementId == id);
            if (achievement == null) return NotFound();
            ViewBag.Categories = await _context.AchievementCategories.ToListAsync();
            return View(achievement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAchievement(Achievement achievement)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            _context.Achievements.Update(achievement);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Achievement updated successfully!";
            return RedirectToAction("Achievements");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAchievement(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            var achievement = await _context.Achievements.FindAsync(id);
            if (achievement != null)
            {
                _context.Achievements.Remove(achievement);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Achievement deleted successfully!";
            }
            return RedirectToAction("Achievements");
        }
        #endregion

        #region AchievementCategories Management
        public async Task<IActionResult> AchievementCategories()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            var categories = await _context.AchievementCategories
                .Include(c => c.Achievements) // ADDED for achievement count
                .ToListAsync();
            return View(categories);
        }

        public IActionResult CreateAchievementCategory()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAchievementCategory(AchievementCategory category)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            _context.AchievementCategories.Add(category);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Achievement Category created successfully!";
            return RedirectToAction("AchievementCategories");
        }

        public async Task<IActionResult> EditAchievementCategory(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            var category = await _context.AchievementCategories
                .Include(c => c.Achievements) // ADDED for achievement count
                .FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAchievementCategory(AchievementCategory category)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            _context.AchievementCategories.Update(category);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Achievement Category updated successfully!";
            return RedirectToAction("AchievementCategories");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAchievementCategory(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            var category = await _context.AchievementCategories.FindAsync(id);
            if (category != null)
            {
                _context.AchievementCategories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Achievement Category deleted successfully!";
            }
            return RedirectToAction("AchievementCategories");
        }
        #endregion

        #region UserWorkouts Management
        public async Task<IActionResult> UserWorkouts()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            var workouts = await _context.UserWorkouts
                .Include(w => w.User)
                .Include(w => w.Exercise)
                .ToListAsync();
            return View(workouts);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUserWorkout(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            var workout = await _context.UserWorkouts.FindAsync(id);
            if (workout != null)
            {
                _context.UserWorkouts.Remove(workout);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "User Workout deleted successfully!";
            }
            return RedirectToAction("UserWorkouts");
        }
        #endregion

        #region UserAchievements Management
        public async Task<IActionResult> UserAchievements()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            var userAchievements = await _context.UserAchievements
                .Include(ua => ua.User)
                .Include(ua => ua.Achievement)
                .ToListAsync();
            return View(userAchievements);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUserAchievement(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            var userAchievement = await _context.UserAchievements.FindAsync(id);
            if (userAchievement != null)
            {
                _context.UserAchievements.Remove(userAchievement);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "User Achievement deleted successfully!";
            }
            return RedirectToAction("UserAchievements");
        }
        #endregion

        #region UserMuscleRankings Management
        public async Task<IActionResult> UserMuscleRankings()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            var rankings = await _context.UserMuscleRankings
                .Include(r => r.User)
                .Include(r => r.MuscleGroup)
                .ToListAsync();
            return View(rankings);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUserMuscleRanking(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            var ranking = await _context.UserMuscleRankings.FindAsync(id);
            if (ranking != null)
            {
                _context.UserMuscleRankings.Remove(ranking);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "User Muscle Ranking deleted successfully!";
            }
            return RedirectToAction("UserMuscleRankings");
        }
        #endregion

        #region WorkoutPlans Management
        public async Task<IActionResult> WorkoutPlans()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            var plans = await _context.WorkoutPlans
                .Include(wp => wp.UserWorkouts) // ADDED for workout count
                .ToListAsync();
            return View(plans);
        }

        public IActionResult CreateWorkoutPlan()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateWorkoutPlan(WorkoutPlan plan)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            if (ModelState.IsValid)
            {
                _context.WorkoutPlans.Add(plan);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Workout plan '{plan.Name}' created successfully!";
                return RedirectToAction("WorkoutPlans");
            }
            
            return View(plan);
        }

        public async Task<IActionResult> EditWorkoutPlan(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            var plan = await _context.WorkoutPlans
                .Include(wp => wp.UserWorkouts) // ADDED for workout count display
                .FirstOrDefaultAsync(wp => wp.WorkoutPlanId == id);
            if (plan == null)
            {
                TempData["ErrorMessage"] = "Workout plan not found.";
                return RedirectToAction("WorkoutPlans");
            }
            return View(plan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditWorkoutPlan(WorkoutPlan plan)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            if (ModelState.IsValid)
            {
                var existingPlan = await _context.WorkoutPlans.FirstOrDefaultAsync(wp => wp.WorkoutPlanId == plan.WorkoutPlanId);
                
                if (existingPlan == null)
                {
                    TempData["ErrorMessage"] = "Workout plan not found.";
                    return RedirectToAction("WorkoutPlans");
                }

                // Update properties
                existingPlan.Name = plan.Name;
                existingPlan.Description = plan.Description;
                existingPlan.Difficulty = plan.Difficulty;
                existingPlan.ImagePath = plan.ImagePath;
                existingPlan.RecommendedDailyFrequency = plan.RecommendedDailyFrequency;
                existingPlan.EstimatedDurationMinutes = plan.EstimatedDurationMinutes;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Workout plan '{plan.Name}' updated successfully!";
                return RedirectToAction("WorkoutPlans");
            }
            
            return View(plan);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteWorkoutPlan(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            
            var plan = await _context.WorkoutPlans
                .Include(wp => wp.UserWorkouts)
                .FirstOrDefaultAsync(wp => wp.WorkoutPlanId == id);
            
            if (plan != null)
            {
                var workoutCount = plan.UserWorkouts.Count;
                var planName = plan.Name;
                
                _context.WorkoutPlans.Remove(plan);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"Workout plan '{planName}' deleted successfully! ({workoutCount} user workout(s) affected)";
            }
            else
            {
                TempData["ErrorMessage"] = "Workout plan not found.";
            }
            
            return RedirectToAction("WorkoutPlans");
        }
        #endregion

        // Helper method to check if user is admin
        private bool IsAdmin()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            return isAdmin == "true";
        }

        // Logout from admin
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("IsAdmin");
            return RedirectToAction("Login", "Auth");
        }
    }
}