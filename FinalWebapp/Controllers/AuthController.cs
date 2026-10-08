using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FinalWebapp.Data;
using FinalWebapp.Models;
using FinalWebapp.ViewModels;

namespace FinalWebapp.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Redirect if already logged in
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string AdminMode = "false")
        {
            // DEBUG: See what's being received
            Console.WriteLine($"AdminMode value: {AdminMode}");
            foreach (var key in Request.Form.Keys)
            {
                Console.WriteLine($"Form Key: {key}, Value: {Request.Form[key]}");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Find user by email or username
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.EmailOrUsername || u.Username == model.EmailOrUsername);

            if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid email/username or password.");
                return View(model);
            }

            // Check if admin mode was selected (accept "true" or "True" or "TRUE")
            if (AdminMode?.ToLower() == "true")
            {
                Console.WriteLine("ADMIN MODE ACTIVATED!"); // Debug
                
                // Set admin session
                HttpContext.Session.SetString("IsAdmin", "true");
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("Username", user.Username);
                
                TempData["SuccessMessage"] = $"Welcome to Admin Panel, {user.Username}!";
                return RedirectToAction("Dashboard", "Admin");
            }

            // Regular user login
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(12)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity), authProperties);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if username or email already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username || u.Email == model.Email);

            if (existingUser != null)
            {
                if (existingUser.Username == model.Username)
                    ModelState.AddModelError("Username", "Username is already taken.");
                if (existingUser.Email == model.Email)
                    ModelState.AddModelError("Email", "Email is already registered.");
                return View(model);
            }

            // Create new user
            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                DateJoined = DateTime.UtcNow,
                ProfilePicture = "/images/default-avatar.png" // Default profile picture
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Auto-login after registration
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = await _context.Users
                .Include(u => u.UserWorkouts)
                .Include(u => u.UserAchievements)
                .Include(u => u.UserMuscleRankings)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user);
        }

        // Helper methods
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "YourSaltHere"));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hashedPassword;
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string Username, string Email)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login");
            }

            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Profile");
                }

                // Validate input
                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email))
                {
                    TempData["ErrorMessage"] = "Username and Email are required.";
                    return RedirectToAction("Profile");
                }

                // Check if username or email is already taken by another user
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id != userId && (u.Username == Username || u.Email == Email));

                if (existingUser != null)
                {
                    if (existingUser.Username == Username)
                        TempData["ErrorMessage"] = "Username is already taken by another user.";
                    else if (existingUser.Email == Email)
                        TempData["ErrorMessage"] = "Email is already registered to another user.";
                    
                    return RedirectToAction("Profile");
                }

                // Update user information
                user.Username = Username;
                user.Email = Email;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating your profile.";
                return RedirectToAction("Profile");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfilePicture(IFormFile profilePicture)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login");
            }

            try
            {
                if (profilePicture == null || profilePicture.Length == 0)
                {
                    TempData["ErrorMessage"] = "Please select a valid image file.";
                    return RedirectToAction("Profile");
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(profilePicture.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    TempData["ErrorMessage"] = "Only JPG, JPEG, PNG, and GIF files are allowed.";
                    return RedirectToAction("Profile");
                }

                // Validate file size (max 5MB)
                if (profilePicture.Length > 5 * 1024 * 1024)
                {
                    TempData["ErrorMessage"] = "File size must be less than 5MB.";
                    return RedirectToAction("Profile");
                }

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Profile");
                }

                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Generate unique filename
                var fileName = $"{userId}_{DateTime.UtcNow.Ticks}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Delete old profile picture if it exists and is not the default
                if (!string.IsNullOrEmpty(user.ProfilePicture) && 
                    user.ProfilePicture != "/images/default-avatar.png" &&
                    user.ProfilePicture.StartsWith("/uploads/"))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePicture.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Save new file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePicture.CopyToAsync(stream);
                }

                // Update user's profile picture path
                user.ProfilePicture = $"/uploads/profiles/{fileName}";
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Profile picture updated successfully!";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while uploading your profile picture.";
                return RedirectToAction("Profile");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAccount()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login");
            }

            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var user = await _context.Users
                    .Include(u => u.UserWorkouts)
                    .Include(u => u.UserAchievements)
                    .Include(u => u.UserMuscleRankings)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                // Delete profile picture if it exists and is not the default
                if (!string.IsNullOrEmpty(user.ProfilePicture) && 
                    user.ProfilePicture != "/images/default-avatar.png" &&
                    user.ProfilePicture.StartsWith("/uploads/"))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePicture.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // Remove user and all related data
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                // Sign out the user
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                TempData["SuccessMessage"] = "Your account has been successfully deleted.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting your account.";
                return RedirectToAction("Profile");
            }
        }

        // API endpoint to get current user info (for AJAX calls)
        [HttpGet]
        public async Task<IActionResult> GetCurrentUserInfo()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Json(new { success = false, message = "Not authenticated" });
            }

            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var user = await _context.Users
                    .Select(u => new {
                        u.Id,
                        u.Username,
                        u.Email,
                        u.ProfilePicture,
                        u.TotalPoints,
                        u.WorkoutCount,
                        u.DateJoined,
                        AchievementCount = u.UserAchievements.Count,
                        MuscleRankingCount = u.UserMuscleRankings.Count
                    })
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                return Json(new { success = true, user });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred" });
            }
        }

        // Add this ViewModel for profile updates
        public class UpdateProfileViewModel
        {
            [Required]
            [StringLength(100, MinimumLength = 3)]
            public string Username { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            [StringLength(255)]
            public string Email { get; set; } = string.Empty;
        }

        // Add this ViewModel for password changes
        public class ChangePasswordViewModel
        {
            [Required]
            [DataType(DataType.Password)]
            public string CurrentPassword { get; set; } = string.Empty;

            [Required]
            [StringLength(255, MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string NewPassword { get; set; } = string.Empty;

            [Required]
            [Compare("NewPassword")]
            [DataType(DataType.Password)]
            public string ConfirmNewPassword { get; set; } = string.Empty;
        }
    }
}