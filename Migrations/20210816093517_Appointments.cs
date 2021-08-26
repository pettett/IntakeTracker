using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IntakeTrackerApp.Migrations
{
	public partial class Appointments : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<bool>(
				name: "MedicalAppointmentNeeded",
				table: "patientReferrals",
				type: "INTEGER",
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "MedicalAppointment_Comment",
				table: "patientReferrals",
				type: "TEXT",
				nullable: false,
				defaultValue: "");

			migrationBuilder.AddColumn<DateTime>(
				name: "MedicalAppointment_Date",
				table: "patientReferrals",
				type: "Date",
				nullable: true);

			migrationBuilder.AddColumn<bool>(
				name: "NursingAppointmentNeeded",
				table: "patientReferrals",
				type: "INTEGER",
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "NursingAppointment_Comment",
				table: "patientReferrals",
				type: "TEXT",
				nullable: false,
				defaultValue: "");

			migrationBuilder.AddColumn<DateTime>(
				name: "NursingAppointment_Date",
				table: "patientReferrals",
				type: "Date",
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "PreviousCorrespondenceSummary",
				table: "patientReferrals",
				type: "TEXT",
				nullable: false,
				defaultValue: "");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "MedicalAppointmentNeeded",
				table: "patientReferrals");

			migrationBuilder.DropColumn(
				name: "MedicalAppointment_Comment",
				table: "patientReferrals");

			migrationBuilder.DropColumn(
				name: "MedicalAppointment_Date",
				table: "patientReferrals");

			migrationBuilder.DropColumn(
				name: "NursingAppointmentNeeded",
				table: "patientReferrals");

			migrationBuilder.DropColumn(
				name: "NursingAppointment_Comment",
				table: "patientReferrals");

			migrationBuilder.DropColumn(
				name: "NursingAppointment_Date",
				table: "patientReferrals");

			migrationBuilder.DropColumn(
				name: "PreviousCorrespondenceSummary",
				table: "patientReferrals");
		}
	}
}
