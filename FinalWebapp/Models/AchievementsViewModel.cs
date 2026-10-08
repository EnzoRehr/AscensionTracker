// Add this to Models/AchievementsViewModel.cs

namespace FinalWebapp.Models
{
    public class AchievementsViewModel
    {
        public List<AchievementCategoryGroup> Categories { get; set; } = new List<AchievementCategoryGroup>();
        public List<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
        public int UserTotalPoints { get; set; }
    }

    public class AchievementCategoryGroup
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Achievement> Achievements { get; set; } = new List<Achievement>();
    }
}
