using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kundenportal.AdminUi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingStructureGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PendingStructureGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Path = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingStructureGroups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PendingStructureGroups_Name_Path",
                table: "PendingStructureGroups",
                columns: new[] { "Name", "Path" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PendingStructureGroups");
        }
    }
}
