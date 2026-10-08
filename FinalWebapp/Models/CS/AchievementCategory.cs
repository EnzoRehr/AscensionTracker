using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace FinalWebapp.Models
{
    public class AchievementCategory
    {
        [Key]
        public int CategoryId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        [StringLength(255)]
        public string IconPath { get; set; }
        
        // Navigation properties
        public virtual ICollection<Achievement> Achievements { get; set; } = new List<Achievement>();
    }
}