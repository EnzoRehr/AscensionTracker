

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalWebapp.Models
{
    public class Achievement
    {
        [Key]
        public int AchievementId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        
        [StringLength(1000)]
        public string Description { get; set; }
        
        [StringLength(255)]
        public string IconPath { get; set; }
        
        [Required]
        public int CategoryId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string RequirementType { get; set; } // WorkoutCount, Weight, Streak, etc.
        
        public int RequirementValue { get; set; }
        
        public int PointsAwarded { get; set; }
        
        // Navigation properties
        [ForeignKey("CategoryId")]
        public virtual AchievementCategory Category { get; set; }
        
        public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
    }
}