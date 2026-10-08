using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace FinalWebapp.Models
{
    public class MuscleGroup
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(255)]
        public string? IconPath { get; set; }  // NULLABLE!
        
        // Navigation properties
        public virtual ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
        public virtual ICollection<UserMuscleRanking> UserMuscleRankings { get; set; } = new List<UserMuscleRanking>();
    }
}
