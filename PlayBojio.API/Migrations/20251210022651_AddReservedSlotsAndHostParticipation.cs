using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayBojio.API.Migrations
{
    /// <inheritdoc />
    public partial class AddReservedSlotsAndHostParticipation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHostParticipating",
                table: "Sessions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ReservedSlots",
                table: "Sessions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHostParticipating",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "ReservedSlots",
                table: "Sessions");
        }
    }
}
