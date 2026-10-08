// Add these to Models/WorkoutViewModels.cs

namespace FinalWebapp.Models
{
    public class StartWorkoutViewModel
    {
        public List<WorkoutPlan> WorkoutPlans { get; set; } = new List<WorkoutPlan>();
        public List<ExerciseDto> Exercises { get; set; } = new List<ExerciseDto>();
    }

    public class ExerciseDto
    {
        public int ExerciseId { get; set; }
        public string Name { get; set; }
        public string MuscleGroupName { get; set; }
    }

    public class CustomWorkoutEntry
    {
        public int ExerciseId { get; set; }
        public int Sets { get; set; }
        public int Reps { get; set; }
    }

    public class ClaimWorkoutRequest
    {
        public int UserWorkoutId { get; set; }
    }
}
