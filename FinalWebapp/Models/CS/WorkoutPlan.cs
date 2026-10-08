using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace FinalWebapp.Models
{
    public class WorkoutPlan
    {
        [Key]
        public int WorkoutPlanId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        
        [StringLength(1000)]
        public string Description { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Difficulty { get; set; } // Beginner, Intermediate, Advanced
        
        [StringLength(500)]
        public string ImagePath { get; set; }
        
        public int RecommendedDailyFrequency { get; set; }
        
        public int EstimatedDurationMinutes { get; set; }
        
        // Navigation properties
        public virtual ICollection<UserWorkout> UserWorkouts { get; set; } = new List<UserWorkout>();
    }
}