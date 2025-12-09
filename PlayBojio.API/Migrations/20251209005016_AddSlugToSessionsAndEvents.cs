using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayBojio.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSlugToSessionsAndEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Sessions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Events",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            // Generate slugs for existing sessions
            migrationBuilder.Sql(@"
                UPDATE Sessions 
                SET Slug = LOWER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
                    Title, ' ', '-'), '''', ''), '!', ''), '@', ''), '#', ''), '$', ''), '%', ''), '^', ''), '&', ''), '*', ''))
                WHERE Slug = '' OR Slug IS NULL
            ");

            // Generate slugs for existing events
            migrationBuilder.Sql(@"
                UPDATE Events 
                SET Slug = LOWER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
                    Name, ' ', '-'), '''', ''), '!', ''), '@', ''), '#', ''), '$', ''), '%', ''), '^', ''), '&', ''), '*', ''))
                WHERE Slug = '' OR Slug IS NULL
            ");

            // Make slugs unique by appending ID if needed
            migrationBuilder.Sql(@"
                UPDATE s1
                SET s1.Slug = s1.Slug + '-' + CAST(s1.Id AS NVARCHAR)
                FROM Sessions s1
                INNER JOIN Sessions s2 ON s1.Slug = s2.Slug AND s1.Id > s2.Id
            ");

            migrationBuilder.Sql(@"
                UPDATE e1
                SET e1.Slug = e1.Slug + '-' + CAST(e1.Id AS NVARCHAR)
                FROM Events e1
                INNER JOIN Events e2 ON e1.Slug = e2.Slug AND e1.Id > e2.Id
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Events");
        }
    }
}
