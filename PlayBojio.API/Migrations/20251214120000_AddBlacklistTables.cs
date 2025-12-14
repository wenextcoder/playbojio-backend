using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PlayBojio.API.Migrations
{
    /// <inheritdoc />
    public partial class AddBlacklistTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventBlacklists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventId = table.Column<int>(type: "integer", nullable: false),
                    BlacklistedUserId = table.Column<string>(type: "text", nullable: false),
                    BlacklistedByUserId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventBlacklists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventBlacklists_AspNetUsers_BlacklistedByUserId",
                        column: x => x.BlacklistedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventBlacklists_AspNetUsers_BlacklistedUserId",
                        column: x => x.BlacklistedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventBlacklists_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupBlacklists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupId = table.Column<int>(type: "integer", nullable: false),
                    BlacklistedUserId = table.Column<string>(type: "text", nullable: false),
                    BlacklistedByUserId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupBlacklists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupBlacklists_AspNetUsers_BlacklistedByUserId",
                        column: x => x.BlacklistedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupBlacklists_AspNetUsers_BlacklistedUserId",
                        column: x => x.BlacklistedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupBlacklists_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventBlacklists_BlacklistedByUserId",
                table: "EventBlacklists",
                column: "BlacklistedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EventBlacklists_BlacklistedUserId",
                table: "EventBlacklists",
                column: "BlacklistedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EventBlacklists_EventId_BlacklistedUserId",
                table: "EventBlacklists",
                columns: new[] { "EventId", "BlacklistedUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupBlacklists_BlacklistedByUserId",
                table: "GroupBlacklists",
                column: "BlacklistedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupBlacklists_BlacklistedUserId",
                table: "GroupBlacklists",
                column: "BlacklistedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupBlacklists_GroupId_BlacklistedUserId",
                table: "GroupBlacklists",
                columns: new[] { "GroupId", "BlacklistedUserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventBlacklists");

            migrationBuilder.DropTable(
                name: "GroupBlacklists");
        }
    }
}
