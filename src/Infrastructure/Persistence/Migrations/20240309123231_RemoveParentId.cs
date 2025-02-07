using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kundenportal.AdminUi.Infrastructure.Persistence.Migrations
{
	/// <inheritdoc />
	public partial class RemoveParentId : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "ParentId",
				table: "StructureGroups");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<Guid>(
				name: "ParentId",
				table: "StructureGroups",
				type: "uuid",
				nullable: true);
		}
	}
}
