using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IntakeTrackerApp.Migrations
{
    public partial class Plans : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConsultantClinicPlan",
                table: "patientReferrals",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Height",
                table: "patientReferrals",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NursingClinicPlan",
                table: "patientReferrals",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProvisinalNursingClinic_Comment",
                table: "patientReferrals",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ProvisinalNursingClinic_Date",
                table: "patientReferrals",
                type: "Date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProvisionalConsultantClinic_Comment",
                table: "patientReferrals",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ProvisionalConsultantClinic_Date",
                table: "patientReferrals",
                type: "Date",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Weight",
                table: "patientReferrals",
                type: "REAL",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsultantClinicPlan",
                table: "patientReferrals");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "patientReferrals");

            migrationBuilder.DropColumn(
                name: "NursingClinicPlan",
                table: "patientReferrals");

            migrationBuilder.DropColumn(
                name: "ProvisinalNursingClinic_Comment",
                table: "patientReferrals");

            migrationBuilder.DropColumn(
                name: "ProvisinalNursingClinic_Date",
                table: "patientReferrals");

            migrationBuilder.DropColumn(
                name: "ProvisionalConsultantClinic_Comment",
                table: "patientReferrals");

            migrationBuilder.DropColumn(
                name: "ProvisionalConsultantClinic_Date",
                table: "patientReferrals");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "patientReferrals");
        }
    }
}
