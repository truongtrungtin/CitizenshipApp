using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionNaturalKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuestionNo",
                table: "Questions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TestVersion",
                table: "Questions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "2025.12");

            migrationBuilder.Sql(
                "WITH cte AS (" +
                "SELECT QuestionId, ROW_NUMBER() OVER (ORDER BY QuestionId) AS rn FROM Questions" +
                ") " +
                "UPDATE q SET QuestionNo = cte.rn FROM Questions q INNER JOIN cte ON q.QuestionId = cte.QuestionId;");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_TestVersion_QuestionNo",
                table: "Questions",
                columns: new[] { "TestVersion", "QuestionNo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Questions_TestVersion_QuestionNo",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "QuestionNo",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "TestVersion",
                table: "Questions");
        }
    }
}
