using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayBojio.API.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoverImageUrl",
                table: "Groups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfileImageUrl",
                table: "Groups",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverImageUrl",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "ProfileImageUrl",
                table: "Groups");
        }
    }
}
