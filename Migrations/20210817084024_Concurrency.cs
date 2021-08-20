using Microsoft.EntityFrameworkCore.Migrations;

namespace IntakeTrackerApp.Migrations
{
    public partial class Concurrency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
    name: "Version",
    table: "patientReferrals",
    type: "INTEGER",
    nullable: false,
    defaultValue: 0);

            migrationBuilder.Sql(@"CREATE TRIGGER UpdateReferralVersion
AFTER UPDATE ON patientReferrals
BEGIN
    UPDATE patientReferrals
    SET Version = Version + 1
    WHERE rowid = NEW.rowid;
            END; ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
    name: "Version",
    table: "patientReferrals");
        }
    }
}
