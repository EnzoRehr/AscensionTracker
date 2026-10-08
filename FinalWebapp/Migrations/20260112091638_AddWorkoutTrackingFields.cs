using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FinalWebapp.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutTrackingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserWorkouts_WorkoutPlans_WorkoutPlanId",
                table: "UserWorkouts");

            migrationBuilder.DeleteData(
                table: "MuscleGroups",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "MuscleGroups",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "MuscleGroups",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "MuscleGroups",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "MuscleGroups",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "MuscleGroups",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "MuscleGroups",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "MuscleGroups",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "MuscleGroups",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "MuscleGroups",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "MuscleGroups",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "MuscleGroups",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "MuscleGroups",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "UserWorkouts");

            migrationBuilder.RenameColumn(
                name: "WorkoutDate",
                table: "UserWorkouts",
                newName: "StartTime");

            migrationBuilder.RenameIndex(
                name: "IX_UserWorkouts_WorkoutDate",
                table: "UserWorkouts",
                newName: "IX_UserWorkouts_StartTime");

            migrationBuilder.AlterColumn<decimal>(
                name: "Weight",
                table: "UserWorkouts",
                type: "decimal(5,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)");

            migrationBuilder.AlterColumn<int>(
                name: "ExerciseId",
                table: "UserWorkouts",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "UserWorkouts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedDurationMinutes",
                table: "UserWorkouts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "UserWorkouts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_UserWorkouts_IsCompleted",
                table: "UserWorkouts",
                column: "IsCompleted");

            migrationBuilder.AddForeignKey(
                name: "FK_UserWorkouts_WorkoutPlans_WorkoutPlanId",
                table: "UserWorkouts",
                column: "WorkoutPlanId",
                principalTable: "WorkoutPlans",
                principalColumn: "WorkoutPlanId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserWorkouts_WorkoutPlans_WorkoutPlanId",
                table: "UserWorkouts");

            migrationBuilder.DropIndex(
                name: "IX_UserWorkouts_IsCompleted",
                table: "UserWorkouts");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "UserWorkouts");

            migrationBuilder.DropColumn(
                name: "EstimatedDurationMinutes",
                table: "UserWorkouts");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "UserWorkouts");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "UserWorkouts",
                newName: "WorkoutDate");

            migrationBuilder.RenameIndex(
                name: "IX_UserWorkouts_StartTime",
                table: "UserWorkouts",
                newName: "IX_UserWorkouts_WorkoutDate");

            migrationBuilder.AlterColumn<decimal>(
                name: "Weight",
                table: "UserWorkouts",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ExerciseId",
                table: "UserWorkouts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "UserWorkouts",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "MuscleGroups",
                columns: new[] { "Id", "Description", "IconPath", "Name" },
                values: new object[,]
                {
                    { 1, "Deltoid muscles including anterior, medial, and posterior heads", null, "Shoulders" },
                    { 2, "Pectoralis major and minor muscles", null, "Chest" },
                    { 3, "Biceps brachii and brachialis muscles", null, "Biceps" },
                    { 4, "Forearm flexors and extensors", null, "Forearms" },
                    { 5, "Triceps brachii muscle", null, "Triceps" },
                    { 6, "Trapezius muscle", null, "Traps" },
                    { 7, "Lower back muscles including erector spinae", null, "Lumbar" },
                    { 8, "Latissimus dorsi muscles", null, "Lats" },
                    { 9, "Muscles involved in grip strength", null, "Grip" },
                    { 10, "Posterior thigh muscles", null, "Hamstrings" },
                    { 11, "Gluteus maximus, medius, and minimus", null, "Glutes" },
                    { 12, "Gastrocnemius and soleus muscles", null, "Calves" },
                    { 13, "Quadriceps femoris muscle group", null, "Quads" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_UserWorkouts_WorkoutPlans_WorkoutPlanId",
                table: "UserWorkouts",
                column: "WorkoutPlanId",
                principalTable: "WorkoutPlans",
                principalColumn: "WorkoutPlanId");
        }
    }
}
