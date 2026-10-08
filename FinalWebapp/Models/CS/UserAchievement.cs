using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace FinalWebapp.Models
{
    public class UserAchievement
    {
        [Key]
        public int UserAchievementId { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public int AchievementId { get; set; }
        
        [Required]
        public DateTime DateEarned { get; set; } = DateTime.UtcNow;
        
        public int PointsEarned { get; set; }
        
        public bool IsDisplayed { get; set; } = true;
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        
        [ForeignKey("AchievementId")]
        public virtual Achievement Achievement { get; set; }
    }
}