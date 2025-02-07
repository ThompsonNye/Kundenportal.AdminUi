using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kundenportal.AdminUi.Infrastructure.Persistence.Migrations
{
	/// <inheritdoc />
	public partial class LimitStringLengths : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<string>(
				name: "Path",
				table: "StructureGroups",
				type: "character varying(256)",
				maxLength: 256,
				nullable: false,
				oldClrType: typeof(string),
				oldType: "text");

			migrationBuilder.AlterColumn<string>(
				name: "Name",
				table: "StructureGroups",
				type: "character varying(64)",
				maxLength: 64,
				nullable: false,
				oldClrType: typeof(string),
				oldType: "text");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<string>(
				name: "Path",
				table: "StructureGroups",
				type: "text",
				nullable: false,
				oldClrType: typeof(string),
				oldType: "character varying(256)",
				oldMaxLength: 256);

			migrationBuilder.AlterColumn<string>(
				name: "Name",
				table: "StructureGroups",
				type: "text",
				nullable: false,
				oldClrType: typeof(string),
				oldType: "character varying(64)",
				oldMaxLength: 64);
		}
	}
}
