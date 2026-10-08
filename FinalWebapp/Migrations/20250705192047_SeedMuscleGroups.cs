using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FinalWebapp.Migrations
{
    /// <inheritdoc />
    public partial class SeedMuscleGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
