using Microsoft.EntityFrameworkCore.Migrations;

namespace IntakeTrackerApp.Migrations
{
	public partial class TransferRegion : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<string>(
				name: "TransferRegion",
				table: "patientReferrals",
				type: "TEXT",
				nullable: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "TransferRegion",
				table: "patientReferrals");
		}
	}
}
