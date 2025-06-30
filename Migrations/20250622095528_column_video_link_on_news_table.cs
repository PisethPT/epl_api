using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace epl_api.Migrations
{
    /// <inheritdoc />
    public partial class column_video_link_on_news_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VideoLink",
                table: "News",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoLink",
                table: "News");
        }
    }
}
