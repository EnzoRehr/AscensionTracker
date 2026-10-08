// Add this to Models/MuscleMapViewModel.cs

namespace FinalWebapp.Models
{
    public class MuscleMapViewModel
    {
        public List<MuscleRankingDto> Rankings { get; set; } = new List<MuscleRankingDto>();
    }

    public class MuscleRankingDto
    {
        public int MuscleGroupId { get; set; }
        public string MuscleName { get; set; }
        public string Rank { get; set; }
        public decimal MaxWeight { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
