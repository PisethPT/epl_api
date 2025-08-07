using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace epl_api.Migrations
{
    /// <inheritdoc />
    public partial class update_match_columns_27_07_25 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                            name: "IsGameFinish",
                            table: "Matches");

            migrationBuilder.DropColumn(
                name: "IsHomeStadium",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "KickoffStatus",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "MatchTime",
                table: "Matches");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<bool>(
    name: "IsGameFinish",
    table: "Matches",
    type: "bit",
    nullable: false,
    defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsHomeStadium",
                table: "Matches",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "KickoffStatus",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "MatchTime",
                table: "Matches",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

        }
    }
}
