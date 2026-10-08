using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FinalWebapp.Data;
using FinalWebapp.Models;

namespace FinalWebapp.Services
{
    public interface IUserService
    {
        Task<User> GetCurrentUserAsync();
        int? GetCurrentUserId();
        bool IsAuthenticated();
    }

    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<User> GetCurrentUserAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return null;

            return await _context.Users
                .Include(u => u.UserWorkouts)
                .Include(u => u.UserAchievements)
                .Include(u => u.UserMuscleRankings)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public int? GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        public bool IsAuthenticated()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
        }
    }
}