using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace FinalWebapp.Models
{
    public class Exercise
    {
        [Key]
        public int ExerciseId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        
        [StringLength(1000)]
        public string Description { get; set; }
        
        [Required]
        public int MuscleGroupId { get; set; }
        
        [StringLength(500)]
        public string VideoUrl { get; set; }
        
        [StringLength(1000)]
        public string Instructions { get; set; }
        
        [StringLength(500)]
        public string ImagePath { get; set; }
        
        // Navigation properties
        [ForeignKey("MuscleGroupId")]
        public virtual MuscleGroup MuscleGroup { get; set; }
        
        public virtual ICollection<UserWorkout> UserWorkouts { get; set; } = new List<UserWorkout>();
    }
}