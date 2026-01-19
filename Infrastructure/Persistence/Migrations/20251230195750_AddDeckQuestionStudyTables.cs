using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDeckQuestionStudyTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Decks",
                columns: table => new
                {
                    DeckId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Decks", x => x.DeckId);
                });

            migrationBuilder.CreateTable(
                name: "StudyEvents",
                columns: table => new
                {
                    StudyEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeckId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SelectedKey = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyEvents", x => x.StudyEventId);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeckId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PromptEn = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    PromptVi = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PromptViPhonetic = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ExplainEn = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ExplainVi = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CorrectOptionKey = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.QuestionId);
                    table.ForeignKey(
                        name: "FK_Questions_Decks_DeckId",
                        column: x => x.DeckId,
                        principalTable: "Decks",
                        principalColumn: "DeckId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionOptions",
                columns: table => new
                {
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TextEn = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TextVi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionOptions", x => new { x.QuestionId, x.Key });
                    table.ForeignKey(
                        name: "FK_QuestionOptions_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Decks_Code",
                table: "Decks",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOptions_QuestionId_SortOrder",
                table: "QuestionOptions",
                columns: new[] { "QuestionId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_DeckId",
                table: "Questions",
                column: "DeckId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyEvents_DeckId_CreatedUtc",
                table: "StudyEvents",
                columns: new[] { "DeckId", "CreatedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_StudyEvents_UserId_CreatedUtc",
                table: "StudyEvents",
                columns: new[] { "UserId", "CreatedUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionOptions");

            migrationBuilder.DropTable(
                name: "StudyEvents");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "Decks");
        }
    }
}
