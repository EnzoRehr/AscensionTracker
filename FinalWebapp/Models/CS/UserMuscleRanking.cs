using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace FinalWebapp.Models
{
    public class UserMuscleRanking
    {
        [Key]
        public int UserMuscleRankingId { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public int MuscleGroupId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Rank { get; set; } // Bronze, Silver, Gold, Platinum, etc.
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal MaxWeight { get; set; }
        
        [Required]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        
        [Required]
        public DateTime RankDate { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        
        [ForeignKey("MuscleGroupId")]
        public virtual MuscleGroup MuscleGroup { get; set; }
    }
}