using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVoiceColumnHotfix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "IF COL_LENGTH('UserSettings', 'Voice') IS NULL " +
                "BEGIN " +
                "ALTER TABLE [UserSettings] ADD [Voice] nvarchar(200) NOT NULL CONSTRAINT [DF_UserSettings_Voice] DEFAULT(''); " +
                "END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "IF COL_LENGTH('UserSettings', 'Voice') IS NOT NULL " +
                "BEGIN " +
                "ALTER TABLE [UserSettings] DROP CONSTRAINT [DF_UserSettings_Voice]; " +
                "ALTER TABLE [UserSettings] DROP COLUMN [Voice]; " +
                "END");
        }
    }
}
