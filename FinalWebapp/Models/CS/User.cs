using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalWebapp.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Username { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }
        
        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; }
        
        [Required]
        public DateTime DateJoined { get; set; } = DateTime.UtcNow;
        
        [StringLength(500)]
        public string ProfilePicture { get; set; }
        
        public int TotalPoints { get; set; } = 0;
        
        public int WorkoutCount { get; set; } = 0;
        
        public DateTime LastWorkoutDate { get; set; }
        
        // Navigation properties
        public virtual ICollection<UserWorkout> UserWorkouts { get; set; } = new List<UserWorkout>();
        public virtual ICollection<UserMuscleRanking> UserMuscleRankings { get; set; } = new List<UserMuscleRanking>();
        public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
    }
}