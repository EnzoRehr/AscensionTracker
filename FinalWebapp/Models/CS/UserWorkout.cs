using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalWebapp.Models
{
    public class UserWorkout
    {
        [Key]
        public int UserWorkoutId { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        // For plan-based workouts (nullable because custom workouts won't have this)
        public int? WorkoutPlanId { get; set; }
        
        // For custom workouts (nullable because plan workouts won't have this)
        public int? ExerciseId { get; set; }
        
        public int Sets { get; set; }
        
        public int Reps { get; set; }
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal? Weight { get; set; }
        
        // NEW FIELDS FOR WORKOUT TRACKING
        [Required]
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        
        public DateTime? EndTime { get; set; }
        
        [Required]
        public int EstimatedDurationMinutes { get; set; }
        
        [Required]
        public int PointsEarned { get; set; }
        
        [Required]
        public bool IsCompleted { get; set; } = false;
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        
        [ForeignKey("WorkoutPlanId")]
        public virtual WorkoutPlan? WorkoutPlan { get; set; }
        
        [ForeignKey("ExerciseId")]
        public virtual Exercise? Exercise { get; set; }
    }
}