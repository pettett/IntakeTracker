using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IntakeTrackerApp.Migrations
{
    public partial class BloodResults : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveReferralActions_Date",
                table: "patientReferrals");

            migrationBuilder.DropColumn(
                name: "BloodTestResults_Date",
                table: "patientReferrals");

            migrationBuilder.RenameColumn(
                name: "BloodTestResults_Comment",
                table: "patientReferrals",
                newName: "BloodTestResults");

            migrationBuilder.RenameColumn(
                name: "ActiveReferralActions_Comment",
                table: "patientReferrals",
                newName: "ActiveReferralActions");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BloodTestResults",
                table: "patientReferrals",
                newName: "BloodTestResults_Comment");

            migrationBuilder.RenameColumn(
                name: "ActiveReferralActions",
                table: "patientReferrals",
                newName: "ActiveReferralActions_Comment");

            migrationBuilder.AddColumn<DateTime>(
                name: "ActiveReferralActions_Date",
                table: "patientReferrals",
                type: "Date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BloodTestResults_Date",
                table: "patientReferrals",
                type: "Date",
                nullable: true);
        }
    }
}
