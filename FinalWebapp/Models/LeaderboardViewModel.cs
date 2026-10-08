// Add this to Models/LeaderboardViewModel.cs

namespace FinalWebapp.Models
{
    public class LeaderboardViewModel
    {
        public List<LeaderboardUserDto> TopUsers { get; set; } = new List<LeaderboardUserDto>();
        public LeaderboardUserDto? CurrentUserRanking { get; set; }
        public int TotalUsers { get; set; }
    }

    public class LeaderboardUserDto
    {
        public int Rank { get; set; }
        public string Username { get; set; }
        public int TotalPoints { get; set; }
        public int WorkoutCount { get; set; }
        public DateTime DateJoined { get; set; }
    }
}
