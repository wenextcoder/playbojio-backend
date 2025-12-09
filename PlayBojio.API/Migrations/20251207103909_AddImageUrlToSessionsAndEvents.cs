using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayBojio.API.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlToSessionsAndEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Sessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Events");
        }
    }
}
