using Microsoft.EntityFrameworkCore.Migrations;

namespace IntakeTrackerApp.Migrations
{
    public partial class KeyChange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NHSNumber",
                table: "patientReferrals",
                newName: "LocalHospitalNumber");

            migrationBuilder.RenameColumn(
                name: "HospitalNumber",
                table: "patientReferrals",
                newName: "NHSNumberKey");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LocalHospitalNumber",
                table: "patientReferrals",
                newName: "NHSNumber");

            migrationBuilder.RenameColumn(
                name: "NHSNumberKey",
                table: "patientReferrals",
                newName: "HospitalNumber");
        }
    }
}
