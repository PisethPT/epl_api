using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace epl_api.Migrations
{
    public partial class update_match_column_matchTime_26_07_25 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the old datetime2 column
            migrationBuilder.DropColumn(
                name: "MatchTime",
                table: "Matches");

            // Add the new time-only column using TimeSpan
            migrationBuilder.AddColumn<TimeSpan>(
                name: "MatchTime",
                table: "Matches",
                type: "time(7)",
                nullable: false,
                defaultValue: TimeSpan.Zero);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert to the original datetime2 column
            migrationBuilder.DropColumn(
                name: "MatchTime",
                table: "Matches");

            migrationBuilder.AddColumn<DateTime>(
                name: "MatchTime",
                table: "Matches",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");
        }
    }
}
