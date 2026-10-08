using Microsoft.EntityFrameworkCore;
using FinalWebapp.Models;

namespace FinalWebapp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        
        public DbSet<User> Users { get; set; }
        public DbSet<MuscleGroup> MuscleGroups { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<WorkoutPlan> WorkoutPlans { get; set; }
        public DbSet<UserWorkout> UserWorkouts { get; set; }
        public DbSet<UserMuscleRanking> UserMuscleRankings { get; set; }
        public DbSet<AchievementCategory> AchievementCategories { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure primary keys
            modelBuilder.Entity<AchievementCategory>()
                .HasKey(ac => ac.CategoryId);
    
            modelBuilder.Entity<Achievement>()
                .HasKey(a => a.AchievementId);
    
            modelBuilder.Entity<Exercise>()
                .HasKey(e => e.ExerciseId);
    
            modelBuilder.Entity<UserWorkout>()
                .HasKey(uw => uw.UserWorkoutId);
    
            modelBuilder.Entity<UserMuscleRanking>()
                .HasKey(umr => umr.UserMuscleRankingId);
    
            modelBuilder.Entity<UserAchievement>()
                .HasKey(ua => ua.UserAchievementId);
    
            modelBuilder.Entity<WorkoutPlan>()
                .HasKey(wp => wp.WorkoutPlanId);
            
            // Configure relationships and constraints
            modelBuilder.Entity<Exercise>()
                .HasOne(e => e.MuscleGroup)
                .WithMany(mg => mg.Exercises)
                .HasForeignKey(e => e.MuscleGroupId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<UserWorkout>()
                .HasOne(uw => uw.User)
                .WithMany(u => u.UserWorkouts)
                .HasForeignKey(uw => uw.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<UserWorkout>()
                .HasOne(uw => uw.Exercise)
                .WithMany(e => e.UserWorkouts)
                .HasForeignKey(uw => uw.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Add relationship for WorkoutPlan
            modelBuilder.Entity<UserWorkout>()
                .HasOne(uw => uw.WorkoutPlan)
                .WithMany(wp => wp.UserWorkouts)
                .HasForeignKey(uw => uw.WorkoutPlanId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<UserMuscleRanking>()
                .HasOne(umr => umr.User)
                .WithMany(u => u.UserMuscleRankings)
                .HasForeignKey(umr => umr.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<UserMuscleRanking>()
                .HasOne(umr => umr.MuscleGroup)
                .WithMany(mg => mg.UserMuscleRankings)
                .HasForeignKey(umr => umr.MuscleGroupId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Achievement>()
                .HasOne(a => a.Category)
                .WithMany(ac => ac.Achievements)
                .HasForeignKey(a => a.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<UserAchievement>()
                .HasOne(ua => ua.User)
                .WithMany(u => u.UserAchievements)
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<UserAchievement>()
                .HasOne(ua => ua.Achievement)
                .WithMany(a => a.UserAchievements)
                .HasForeignKey(ua => ua.AchievementId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Add indexes for better performance
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
            
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            
            // FIXED: Changed from WorkoutDate to StartTime
            modelBuilder.Entity<UserWorkout>()
                .HasIndex(uw => uw.StartTime);
            
            // Add index for IsCompleted for faster queries
            modelBuilder.Entity<UserWorkout>()
                .HasIndex(uw => uw.IsCompleted);
            
            modelBuilder.Entity<UserMuscleRanking>()
                .HasIndex(umr => new { umr.UserId, umr.MuscleGroupId })
                .IsUnique();
        }
    }
}