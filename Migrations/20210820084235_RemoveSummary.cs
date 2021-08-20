using Microsoft.EntityFrameworkCore.Migrations;

namespace IntakeTrackerApp.Migrations
{
    public partial class RemoveSummary : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Summary_LastName",
                table: "patientReferrals",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "Summary_FirstName",
                table: "patientReferrals",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "Summary_DateOfBirth",
                table: "patientReferrals",
                newName: "DateOfBirth");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "patientReferrals",
                newName: "Summary_LastName");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "patientReferrals",
                newName: "Summary_FirstName");

            migrationBuilder.RenameColumn(
                name: "DateOfBirth",
                table: "patientReferrals",
                newName: "Summary_DateOfBirth");
        }
    }
}
