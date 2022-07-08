﻿// <auto-generated />
using System;
using IntakeTrackerApp.DataManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace IntakeTrackerApp.Migrations
{
    [DbContext(typeof(PatientsContext))]
    [Migration("20210816133726_PreferredContact")]
    partial class PreferredContact
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0-preview.7.21378.4");

            modelBuilder.Entity("IntakeTrackerApp.PatientReferral", b =>
                {
                    b.Property<ulong>("HospitalNumber")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ActiveReferralActions")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("Archived")
                        .HasColumnType("INTEGER");

                    b.Property<bool?>("BloodTestNeeded")
                        .HasColumnType("INTEGER");

                    b.Property<string>("BloodTestResults")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("BriefDetails")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateOnReferral")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateReferralReceived")
                        .HasColumnType("TEXT");

                    b.Property<bool?>("MedicalAppointmentNeeded")
                        .HasColumnType("INTEGER");

                    b.Property<string>("NHSNumber")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool?>("NursingAppointmentNeeded")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PreferredContactMethod")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool?>("PreviousCorrespondenceNeeded")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PreviousCorrespondenceSummary")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ReferralType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ResponsibleOfActiveManagement")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("HospitalNumber");

                    b.ToTable("patientReferrals");
                });

            modelBuilder.Entity("IntakeTrackerApp.PatientReferral", b =>
                {
                    b.OwnsOne("IntakeTrackerApp.DateRecord", "BloodFormsSent", b1 =>
                        {
                            b1.Property<ulong>("PatientReferralHospitalNumber")
                                .HasColumnType("INTEGER");

                            b1.Property<string>("Comment")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<DateTime?>("Date")
                                .HasColumnType("Date");

                            b1.HasKey("PatientReferralHospitalNumber");

                            b1.ToTable("patientReferrals");

                            b1.WithOwner()
                                .HasForeignKey("PatientReferralHospitalNumber");
                        });

                    b.OwnsOne("IntakeTrackerApp.DateRecord", "BloodTestPlanned", b1 =>
                        {
                            b1.Property<ulong>("PatientReferralHospitalNumber")
                                .HasColumnType("INTEGER");

                            b1.Property<string>("Comment")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<DateTime?>("Date")
                                .HasColumnType("Date");

                            b1.HasKey("PatientReferralHospitalNumber");

                            b1.ToTable("patientReferrals");

                            b1.WithOwner()
                                .HasForeignKey("PatientReferralHospitalNumber");
                        });

                    b.OwnsOne("IntakeTrackerApp.DateRecord", "BloodTestReported", b1 =>
                        {
                            b1.Property<ulong>("PatientReferralHospitalNumber")
                                .HasColumnType("INTEGER");

                            b1.Property<string>("Comment")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<DateTime?>("Date")
                                .HasColumnType("Date");

                            b1.HasKey("PatientReferralHospitalNumber");

                            b1.ToTable("patientReferrals");

                            b1.WithOwner()
                                .HasForeignKey("PatientReferralHospitalNumber");
                        });

                    b.OwnsOne("IntakeTrackerApp.DateRecord", "ContactAttempted", b1 =>
                        {
                            b1.Property<ulong>("PatientReferralHospitalNumber")
                                .HasColumnType("INTEGER");

                            b1.Property<string>("Comment")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<DateTime?>("Date")
                                .HasColumnType("Date");

                            b1.HasKey("PatientReferralHospitalNumber");

                            b1.ToTable("patientReferrals");

                            b1.WithOwner()
                                .HasForeignKey("PatientReferralHospitalNumber");
                        });

                    b.OwnsOne("IntakeTrackerApp.DateRecord", "DateContactMade", b1 =>
                        {
                            b1.Property<ulong>("PatientReferralHospitalNumber")
                                .HasColumnType("INTEGER");

                            b1.Property<string>("Comment")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<DateTime?>("Date")
                                .HasColumnType("Date");

                            b1.HasKey("PatientReferralHospitalNumber");

                            b1.ToTable("patientReferrals");

                            b1.WithOwner()
                                .HasForeignKey("PatientReferralHospitalNumber");
                        });

                    b.OwnsOne("IntakeTrackerApp.DateRecord", "DateOfActiveManagement", b1 =>
                        {
                            b1.Property<ulong>("PatientReferralHospitalNumber")
                                .HasColumnType("INTEGER");

                            b1.Property<string>("Comment")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<DateTime?>("Date")
                                .HasColumnType("Date");

                            b1.HasKey("PatientReferralHospitalNumber");

                            b1.ToTable("patientReferrals");

                            b1.WithOwner()
                                .HasForeignKey("PatientReferralHospitalNumber");
                        });

                    b.OwnsOne("IntakeTrackerApp.Test", "EP", b1 =>
                        {
                            b1.Property<ulong>("PatientReferralHospitalNumber")
                                .HasColumnType("INTEGER");

                            b1.Property<string>("Name")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<bool?>("Needed")
                                .HasColumnType("INTEGER");

                            b1.HasKey("PatientReferralHospitalNumber");

                            b1.ToTable("patientReferrals");

                            b1.WithOwner()
                                .HasForeignKey("PatientReferralHospitalNumber");

                            b1.OwnsOne("IntakeTrackerApp.DateRecord", "ReportedDate", b2 =>
                                {
                                    b2.Property<ulong>("TestPatientReferralHospitalNumber")
                                        .HasColumnType("INTEGER");

                                    b2.Property<string>("Comment")
                                        .IsRequired()
                                        .HasColumnType("TEXT");

                                    b2.Property<DateTime?>("Date")
                                        .HasColumnType("Date");

                                    b2.HasKey("TestPatientReferralHospitalNumber");

                                    b2.ToTable("patientReferrals");

                                    b2.WithOwner()
                                        .HasForeignKey("TestPatientReferralHospitalNumber");
                                });

                            b1.OwnsOne("IntakeTrackerApp.DateRecord", "RequestedDate", b2 =>
                                {
                                    b2.Property<ulong>("TestPatientReferralHospitalNumber")
                                        .HasColumnType("INTEGER");

                                    b2.Property<string>("Comment")
                                        .IsRequired()
                                        .HasColumnType("TEXT");

                                    b2.Property<DateTime?>("Date")
                                        .HasColumnType("Date");

                                    b2.HasKey("TestPatientReferralHospitalNumber");

                                    b2.ToTable("patientReferrals");

                                    b2.WithOwner()
                                        .HasForeignKey("TestPatientReferralHospitalNumber");
                                });

                            b1.OwnsOne("IntakeTrackerApp.DateRecord", "TestDate", b2 =>
                                {
                                    b2.Property<ulong>("TestPatientReferralHospitalNumber")
                                        .HasColumnType("INTEGER");

                                    b2.Property<string>("Comment")
                                        .IsRequired()
                                        .HasColumnType("TEXT");

                                    b2.Property<DateTime?>("Date")
                                        .HasColumnType("Date");

                                    b2.HasKey("TestPatientReferralHospitalNumber");

                                    b2.ToTable("patientReferrals");

                                    b2.WithOwner()
                                        .HasForeignKey("TestPatientReferralHospitalNumber");
                                });

                            b1.Navigation("ReportedDate")
                                .IsRequired();

                            b1.Navigation("RequestedDate")
                                .IsRequired();

                            b1.Navigation("TestDate")
                                .IsRequired();
                        });

                    b.OwnsOne("IntakeTrackerApp.Test", "LP", b1 =>
                        {
                            b1.Property<ulong>("PatientReferralHospitalNumber")
                                .HasColumnType("INTEGER");

                            b1.Property<string>("Name")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<bool?>("Needed")
                                .HasColumnType("INTEGER");

                            b1.HasKey("PatientReferralHospitalNumber");

                            b1.ToTable("patientReferrals");

                            b1.WithOwner()
                                .HasForeignKey("PatientReferralHospitalNumber");

                            b1.OwnsOne("IntakeTrackerApp.DateRecord", "ReportedDate", b2 =>
                                {
                                    b2.Property<ulong>("TestPatientReferralHospitalNumber")
                                        .HasColumnType("INTEGER");

                                    b2.Property<string>("Comment")
                                        .IsRequired()
                                        .HasColumnType("TEXT");

                                    b2.Property<DateTime?>("Date")
                                        .HasColumnType("Date");

                                    b2.HasKey("TestPatientReferralHospitalNumber");

                                    b2.ToTable("patientReferrals");

                                    b2.WithOwner()
                                        .HasForeignKey("TestPatientReferralHospitalNumber");
                                });

                            b1.OwnsOne("IntakeTrackerApp.DateRecord", "RequestedDate", b2 =>
                                {
                                    b2.Property<ulong>("TestPatientReferralHospitalNumber")
                                        .HasColumnType("INTEGER");

                                    b2.Property<string>("Comment")
                                        .IsRequired()
                                        .HasColumnType("TEXT");

                                    b2.Property<DateTime?>("Date")
                                        .HasColumnType("Date");

                                    b2.HasKey("TestPatientReferralHospitalNumber");

                                    b2.ToTable("patientReferrals");

                                    b2.WithOwner()
                                        .HasForeignKey("TestPatientReferralHospitalNumber");
                                });

                            b1.OwnsOne("IntakeTrackerApp.DateRecord", "TestDate", b2 =>
                                {
                                    b2.Property<ulong>("TestPatientReferralHospitalNumber")
                                        .HasColumnType("INTEGER");

                                    b2.Property<string>("Comment")
                                        .IsRequired()
                                        .HasColumnType("TEXT");

                                    b2.Property<DateTime?>("Date")
                                        .HasColumnType("Date");

                                    b2.HasKey("TestPatientReferralHospitalNumber");

                                    b2.ToTable("patientReferrals");

                                    b2.WithOwner()
                                        .HasForeignKey("TestPatientReferralHospitalNumber");
                                });

                            b1.Navigation("ReportedDate")
                                .IsRequired();

                            b1.Navigation("RequestedDate")
                                .IsRequired();

                            b1.Navigation("TestDate")
                                .IsRequired();
                        });

                    b.OwnsOne("IntakeTrackerApp.Test", "MRI", b1 =>
                        {
                            b1.Property<ulong>("PatientReferralHospitalNumber")
                                .HasColumnType("INTEGER");

                            b1.Property<string>("Name")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<bool?>("Needed")
                                .HasColumnType("INTEGER");

                            b1.HasKey("PatientReferralHospitalNumber");

                            b1.ToTable("patientReferrals");

                            b1.WithOwner()
                                .HasForeignKey("PatientReferralHospitalNumber");

                            b1.OwnsOne("IntakeTrackerApp.DateRecord", "ReportedDate", b2 =>
                                {
                                    b2.Property<ulong>("TestPatientReferralHospitalNumber")
                                        .HasColumnType("INTEGER");

                                    b2.Property<string>("Comment")
                                        .IsRequired()
                                        .HasColumnType("TEXT");

                                    b2.Property<DateTime?>("Date")
                                        .HasColumnType("Date");

                                    b2.HasKey("TestPatientReferralHospitalNumber");

                                    b2.ToTable("patientReferrals");

                                    b2.WithOwner()
                                        .HasForeignKey("TestPatientReferralHospitalNumber");
                                });

                            b1.OwnsOne("IntakeTrackerApp.DateRecord", "RequestedDate", b2 =>
                                {
                                    b2.Property<ulong>("TestPatientReferralHospitalNumber")
                                        .HasColumnType("INTEGER");

                                    b2.Property<string>("Comment")
                                        .IsRequired()
                                        .HasColumnType("TEXT");

                                    b2.Property<DateTime?>("Date")
                                        .HasColumnType("Date");

                                    b2.HasKey("TestPatientReferralHospitalNumber");

                                    b2.ToTable("patientReferrals");

                                    b2.WithOwner()
                                        .HasForeignKey("TestPatientReferralHospitalNumber");
                                });

                            b1.OwnsOne("IntakeTrackerApp.DateRecord", "TestDate", b2 =>
                                {
                                    b2.Property<ulong>("TestPatientReferralHospitalNumber")
                                        .HasColumnType("INTEGER");

                                    b2.Property<string>("Comment")
                                        .IsRequired()
                                        .HasColumnType("TEXT");

                                    b2.Property<DateTime?>("Date")
                                        .HasColumnType("Date");

                                    b2.HasKey("TestPatientReferralHospitalNumber");

                                    b2.ToTable("patientReferrals");

                                    b2.WithOwner()
                                        .HasForeignKey("TestPatientReferralHospitalNumber");
                                });

                            b1.Navigation("ReportedDate")
                                .IsRequired();

                            b1.Navigation("RequestedDate")
                                .IsRequired();

                            b1.Navigation("TestDate")
                                .IsRequired();
                        });

                    b.OwnsOne("IntakeTrackerApp.DateRecord", "MedicalAppointment", b1 =>
                        {
                            b1.Property<ulong>("PatientReferralHospitalNumber")
                                .HasColumnType("INTEGER");

                            b1.Property<string>("Comment")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<DateTime?>("Date")
                                .HasColumnType("Date");

                            b1.HasKey("PatientReferralHospitalNumber");

                            b1.ToTable("patientReferrals");

                            b1.WithOwner()
                                .HasForeignKey("PatientReferralHospitalNumber");
                        });

                    b.OwnsOne("IntakeTrackerApp.DateRecord", "NursingAppointment", b1 =>
                        {
                            b1.Property<ulong>("PatientReferralHospitalNumber")
                                .HasColumnType("INTEGER");

                            b1.Property<string>("Comment")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<DateTime?>("Date")
                                .HasColumnType("Date");

                            b1.HasKey("PatientReferralHospitalNumber");

                            b1.ToTable("patientReferrals");

                            b1.WithOwner()
                                .HasForeignKey("PatientReferralHospitalNumber");
                        });

                    b.OwnsOne("IntakeTrackerApp.DateRecord", "PreviousCorrespondenceReceived", b1 =>
                        {
                            b1.Property<ulong>("PatientReferralHospitalNumber")
                                .HasColumnType("INTEGER");

                            b1.Property<string>("Comment")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<DateTime?>("Date")
                                .HasColumnType("Date");

                            b1.HasKey("PatientReferralHospitalNumber");

                            b1.ToTable("patientReferrals");

                            b1.WithOwner()
                                .HasForeignKey("PatientReferralHospitalNumber");
                        });

                    b.OwnsOne("IntakeTrackerApp.DateRecord", "PreviousCorrespondenceRequested", b1 =>
                        {
                            b1.Property<ulong>("PatientReferralHospitalNumber")
                                .HasColumnType("INTEGER");

                            b1.Property<string>("Comment")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<DateTime?>("Date")
                                .HasColumnType("Date");

                            b1.HasKey("PatientReferralHospitalNumber");

                            b1.ToTable("patientReferrals");

                            b1.WithOwner()
                                .HasForeignKey("PatientReferralHospitalNumber");
                        });

                    b.OwnsOne("IntakeTrackerApp.SummaryInfomation", "Summary", b1 =>
                        {
                            b1.Property<ulong>("PatientReferralHospitalNumber")
                                .HasColumnType("INTEGER");

                            b1.Property<DateTime>("DateOfBirth")
                                .HasColumnType("TEXT");

                            b1.Property<string>("FirstName")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<string>("LastName")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.HasKey("PatientReferralHospitalNumber");

                            b1.ToTable("patientReferrals");

                            b1.WithOwner()
                                .HasForeignKey("PatientReferralHospitalNumber");
                        });

                    b.Navigation("BloodFormsSent")
                        .IsRequired();

                    b.Navigation("BloodTestPlanned")
                        .IsRequired();

                    b.Navigation("BloodTestReported")
                        .IsRequired();

                    b.Navigation("ContactAttempted")
                        .IsRequired();

                    b.Navigation("DateContactMade")
                        .IsRequired();

                    b.Navigation("DateOfActiveManagement")
                        .IsRequired();

                    b.Navigation("EP")
                        .IsRequired();

                    b.Navigation("LP")
                        .IsRequired();

                    b.Navigation("MRI")
                        .IsRequired();

                    b.Navigation("MedicalAppointment")
                        .IsRequired();

                    b.Navigation("NursingAppointment")
                        .IsRequired();

                    b.Navigation("PreviousCorrespondenceReceived")
                        .IsRequired();

                    b.Navigation("PreviousCorrespondenceRequested")
                        .IsRequired();

                    b.Navigation("Summary")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
