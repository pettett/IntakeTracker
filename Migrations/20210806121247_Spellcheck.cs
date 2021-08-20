using Microsoft.EntityFrameworkCore.Migrations;

namespace IntakeTrackerApp.Migrations
{
    public partial class Spellcheck : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PreviousCorrispondanceRequested_Date",
                table: "patientReferrals",
                newName: "PreviousCorrespondenceRequested_Date");

            migrationBuilder.RenameColumn(
                name: "PreviousCorrispondanceRequested_Comment",
                table: "patientReferrals",
                newName: "PreviousCorrespondenceRequested_Comment");

            migrationBuilder.RenameColumn(
                name: "PreviousCorrispondanceRecieved_Date",
                table: "patientReferrals",
                newName: "PreviousCorrespondenceReceived_Date");

            migrationBuilder.RenameColumn(
                name: "PreviousCorrispondanceRecieved_Comment",
                table: "patientReferrals",
                newName: "PreviousCorrespondenceReceived_Comment");

            migrationBuilder.RenameColumn(
                name: "PreviousCorrispondanceNeeded",
                table: "patientReferrals",
                newName: "PreviousCorrespondenceNeeded");

            migrationBuilder.RenameColumn(
                name: "DateReferralRecieved",
                table: "patientReferrals",
                newName: "DateReferralReceived");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PreviousCorrespondenceRequested_Date",
                table: "patientReferrals",
                newName: "PreviousCorrispondanceRequested_Date");

            migrationBuilder.RenameColumn(
                name: "PreviousCorrespondenceRequested_Comment",
                table: "patientReferrals",
                newName: "PreviousCorrispondanceRequested_Comment");

            migrationBuilder.RenameColumn(
                name: "PreviousCorrespondenceReceived_Date",
                table: "patientReferrals",
                newName: "PreviousCorrispondanceRecieved_Date");

            migrationBuilder.RenameColumn(
                name: "PreviousCorrespondenceReceived_Comment",
                table: "patientReferrals",
                newName: "PreviousCorrispondanceRecieved_Comment");

            migrationBuilder.RenameColumn(
                name: "PreviousCorrespondenceNeeded",
                table: "patientReferrals",
                newName: "PreviousCorrispondanceNeeded");

            migrationBuilder.RenameColumn(
                name: "DateReferralReceived",
                table: "patientReferrals",
                newName: "DateReferralRecieved");
        }
    }
}
