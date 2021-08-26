using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IntakeTrackerApp.Migrations
{
	public partial class InitialCreate : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "patientReferrals",
				columns: table => new
				{
					HospitalNumber = table.Column<ulong>(type: "INTEGER", nullable: false)
						.Annotation("Sqlite:Autoincrement", true),
					Summary_FirstName = table.Column<string>(type: "TEXT", nullable: false),
					Summary_LastName = table.Column<string>(type: "TEXT", nullable: false),
					Summary_DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: false),
					DateOnReferral = table.Column<DateTime>(type: "TEXT", nullable: false),
					DateReferralRecieved = table.Column<DateTime>(type: "TEXT", nullable: false),
					ReferralType = table.Column<string>(type: "TEXT", nullable: false),
					BriefDetails = table.Column<string>(type: "TEXT", nullable: false),
					DateOfActiveManagement_Comment = table.Column<string>(type: "TEXT", nullable: false),
					DateOfActiveManagement_Date = table.Column<DateTime>(type: "Date", nullable: true),
					ResponsibleOfActiveManagement = table.Column<string>(type: "TEXT", nullable: false),
					ActiveReferralActions_Comment = table.Column<string>(type: "TEXT", nullable: false),
					ActiveReferralActions_Date = table.Column<DateTime>(type: "Date", nullable: true),
					ContactAttempted_Comment = table.Column<string>(type: "TEXT", nullable: false),
					ContactAttempted_Date = table.Column<DateTime>(type: "Date", nullable: true),
					DateContactMade_Comment = table.Column<string>(type: "TEXT", nullable: false),
					DateContactMade_Date = table.Column<DateTime>(type: "Date", nullable: true),
					MRI_RequestedDate_Comment = table.Column<string>(type: "TEXT", nullable: false),
					MRI_RequestedDate_Date = table.Column<DateTime>(type: "Date", nullable: true),
					MRI_TestDate_Comment = table.Column<string>(type: "TEXT", nullable: false),
					MRI_TestDate_Date = table.Column<DateTime>(type: "Date", nullable: true),
					MRI_ReportedDate_Comment = table.Column<string>(type: "TEXT", nullable: false),
					MRI_ReportedDate_Date = table.Column<DateTime>(type: "Date", nullable: true),
					MRI_Name = table.Column<string>(type: "TEXT", nullable: false),
					MRI_Needed = table.Column<bool>(type: "INTEGER", nullable: true),
					LP_RequestedDate_Comment = table.Column<string>(type: "TEXT", nullable: false),
					LP_RequestedDate_Date = table.Column<DateTime>(type: "Date", nullable: true),
					LP_TestDate_Comment = table.Column<string>(type: "TEXT", nullable: false),
					LP_TestDate_Date = table.Column<DateTime>(type: "Date", nullable: true),
					LP_ReportedDate_Comment = table.Column<string>(type: "TEXT", nullable: false),
					LP_ReportedDate_Date = table.Column<DateTime>(type: "Date", nullable: true),
					LP_Name = table.Column<string>(type: "TEXT", nullable: false),
					LP_Needed = table.Column<bool>(type: "INTEGER", nullable: true),
					EP_RequestedDate_Comment = table.Column<string>(type: "TEXT", nullable: false),
					EP_RequestedDate_Date = table.Column<DateTime>(type: "Date", nullable: true),
					EP_TestDate_Comment = table.Column<string>(type: "TEXT", nullable: false),
					EP_TestDate_Date = table.Column<DateTime>(type: "Date", nullable: true),
					EP_ReportedDate_Comment = table.Column<string>(type: "TEXT", nullable: false),
					EP_ReportedDate_Date = table.Column<DateTime>(type: "Date", nullable: true),
					EP_Name = table.Column<string>(type: "TEXT", nullable: false),
					EP_Needed = table.Column<bool>(type: "INTEGER", nullable: true),
					BloodTestNeeded = table.Column<bool>(type: "INTEGER", nullable: true),
					BloodFormsSent_Comment = table.Column<string>(type: "TEXT", nullable: false),
					BloodFormsSent_Date = table.Column<DateTime>(type: "Date", nullable: true),
					BloodTestPlanned_Comment = table.Column<string>(type: "TEXT", nullable: false),
					BloodTestPlanned_Date = table.Column<DateTime>(type: "Date", nullable: true),
					BloodTestReported_Comment = table.Column<string>(type: "TEXT", nullable: false),
					BloodTestReported_Date = table.Column<DateTime>(type: "Date", nullable: true),
					BloodTestResults_Comment = table.Column<string>(type: "TEXT", nullable: false),
					BloodTestResults_Date = table.Column<DateTime>(type: "Date", nullable: true),
					PreviousCorrispondanceRequested_Comment = table.Column<string>(type: "TEXT", nullable: false),
					PreviousCorrispondanceRequested_Date = table.Column<DateTime>(type: "Date", nullable: true),
					PreviousCorrispondanceRecieved_Comment = table.Column<string>(type: "TEXT", nullable: false),
					PreviousCorrispondanceRecieved_Date = table.Column<DateTime>(type: "Date", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_patientReferrals", x => x.HospitalNumber);
				});
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "patientReferrals");
		}
	}
}
