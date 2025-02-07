using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kundenportal.AdminUi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMinimalPendingStructureGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PendingStructureGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingStructureGroups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PendingStructureGroups_Name",
                table: "PendingStructureGroups",
                column: "Name",
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
