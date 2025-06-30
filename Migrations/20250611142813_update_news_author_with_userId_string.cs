using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace epl_api.Migrations
{
    /// <inheritdoc />
    public partial class update_news_author_with_userId_string : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_News_AspNetUsers_UserId1",
                table: "News");

            migrationBuilder.DropIndex(
                name: "IX_News_UserId1",
                table: "News");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "News");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "News",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_News_UserId",
                table: "News",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_News_AspNetUsers_UserId",
                table: "News",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_News_AspNetUsers_UserId",
                table: "News");

            migrationBuilder.DropIndex(
                name: "IX_News_UserId",
                table: "News");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "News",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "News",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_News_UserId1",
                table: "News",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_News_AspNetUsers_UserId1",
                table: "News",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
