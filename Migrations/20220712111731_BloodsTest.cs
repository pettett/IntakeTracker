using Microsoft.EntityFrameworkCore.Migrations;

namespace IntakeTrackerApp.Migrations
{
    public partial class BloodsTest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.RenameColumn(
         name: "BloodFormsSent_Date",
         table: "patientReferrals",
         newName: "Bloods_RequestedDate_Date");

            migrationBuilder.RenameColumn(
                name: "BloodFormsSent_Comment",
                table: "patientReferrals",
                newName: "Bloods_RequestedDate_Comment");


            migrationBuilder.RenameColumn(
                name: "BloodTestPlanned_Date",
                table: "patientReferrals",
                newName: "Bloods_TestDate_Date");

            migrationBuilder.RenameColumn(
                name: "BloodTestPlanned_Comment",
                table: "patientReferrals",
                newName: "Bloods_TestDate_Comment");



            migrationBuilder.RenameColumn(
                name: "BloodTestReported_Date",
                table: "patientReferrals",
                newName: "Bloods_ReportedDate_Date");

            migrationBuilder.RenameColumn(
                name: "BloodTestReported_Comment",
                table: "patientReferrals",
                newName: "Bloods_ReportedDate_Comment");

            migrationBuilder.RenameColumn(
                name: "BloodTestNeeded",
                table: "patientReferrals",
                newName: "Bloods_Needed");

            migrationBuilder.AddColumn<string>(
                name: "Bloods_Name",
                table: "patientReferrals",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bloods_Name",
                table: "patientReferrals");

            migrationBuilder.RenameColumn(
                name: "Bloods_TestDate_Date",
                table: "patientReferrals",
                newName: "BloodTestPlanned_Date");

            migrationBuilder.RenameColumn(
                name: "Bloods_TestDate_Comment",
                table: "patientReferrals",
                newName: "BloodTestPlanned_Comment");

            migrationBuilder.RenameColumn(
                name: "Bloods_RequestedDate_Date",
                table: "patientReferrals",
                newName: "BloodFormsSent_Date");

            migrationBuilder.RenameColumn(
                name: "Bloods_RequestedDate_Comment",
                table: "patientReferrals",
                newName: "BloodFormsSent_Comment");

            migrationBuilder.RenameColumn(
                name: "Bloods_ReportedDate_Date",
                table: "patientReferrals",
                newName: "BloodTestReported_Date");

            migrationBuilder.RenameColumn(
                name: "Bloods_ReportedDate_Comment",
                table: "patientReferrals",
                newName: "BloodTestReported_Comment");

            migrationBuilder.RenameColumn(
                name: "Bloods_Needed",
                table: "patientReferrals",
                newName: "BloodTestNeeded");
        }
    }
}
