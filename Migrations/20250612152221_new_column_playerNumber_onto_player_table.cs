using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace epl_api.Migrations
{
    /// <inheritdoc />
    public partial class new_column_playerNumber_onto_player_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlayerNumber",
                table: "Players",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlayerNumber",
                table: "Players");
        }
    }
}
