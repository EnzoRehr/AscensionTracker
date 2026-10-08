// Add this file to: Models/HomeViewModel.cs

namespace FinalWebapp.Models
{
    public class HomeViewModel
    {
        public bool HasCompletedAssessment { get; set; }
        public string? ChestRank { get; set; }
        public string? BicepsRank { get; set; }
        public string? Username { get; set; }
    }
}
