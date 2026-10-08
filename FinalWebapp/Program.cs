using Microsoft.EntityFrameworkCore;
using FinalWebapp.Data;
using FinalWebapp.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using FinalWebapp.Services;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ ADD AUTHENTICATION SERVICES BEFORE builder.Build()
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(12);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

// ✅ ADD SESSION SUPPORT (REQUIRED FOR ADMIN MODE)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ ADD CUSTOM SERVICES BEFORE builder.Build()
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddHttpContextAccessor();

// ✅ BUILD THE APP AFTER ALL SERVICES ARE REGISTERED
var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ ADD AUTHENTICATION MIDDLEWARE AFTER app.Build()
app.UseAuthentication();

// ✅ ADD SESSION MIDDLEWARE (REQUIRED FOR ADMIN MODE)
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ✅ DATABASE SEEDING AND VERIFICATION
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
        
        // Check Muscle Groups
        var muscleGroupCount = await context.MuscleGroups.CountAsync();
        Console.WriteLine($"💪 Total muscle groups: {muscleGroupCount}");
        
        if (muscleGroupCount > 0)
        {
            var allGroups = await context.MuscleGroups.OrderBy(mg => mg.Name).ToListAsync();
            Console.WriteLine("📋 Current muscle groups:");
            foreach (var group in allGroups)
            {
                Console.WriteLine($"  - ID: {group.Id}, Name: '{group.Name}'");
            }
        }
        
        // Check if Abdominals exists, if not add it
        var hasAbdominals = await context.MuscleGroups.AnyAsync(mg => mg.Name == "Abdominals");
        if (!hasAbdominals)
        {
            var abdominals = new MuscleGroup 
            { 
                Name = "Abdominals", 
                Description = "Rectus abdominis, internal and external obliques, and transverse abdominis" 
            };
            
            context.MuscleGroups.Add(abdominals);
            await context.SaveChangesAsync();
            Console.WriteLine("✅ Added Abdominals muscle group!");
        }
        
        // Check Users
        var userCount = await context.Users.CountAsync();
        Console.WriteLine($"👥 Total users in database: {userCount}");
        
        if (userCount > 0)
        {
            var recentUsers = await context.Users
                .OrderByDescending(u => u.DateJoined)
                .Take(3)
                .Select(u => new { u.Username, u.DateJoined })
                .ToListAsync();
            
            Console.WriteLine("📋 Recent users:");
            foreach (var user in recentUsers)
            {
                Console.WriteLine($"  - {user.Username} (joined: {user.DateJoined:yyyy-MM-dd})");
            }
        }
        else
        {
            Console.WriteLine("ℹ️  No users registered yet. Visit /Auth/Register to create an account.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error during startup: {ex.Message}");
    }
}

Console.WriteLine("🚀 Application started successfully!");
Console.WriteLine("🛡️  Admin mode enabled - check the admin box on login to access database management!");


app.Run();

// Helper method for password hashing (if you want to create test users)
static string HashPassword(string password)
{
    using var sha256 = SHA256.Create();
    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "YourSaltHere"));
    return Convert.ToBase64String(hashedBytes);
}