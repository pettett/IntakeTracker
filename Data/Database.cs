using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.IO;
using Microsoft.EntityFrameworkCore.Design;

namespace IntakeTrackerApp.Data;
public class PatientsContext : DbContext
{
    //pinkly promise this value is not null
    public DbSet<PatientReferral> patientReferrals { get; set; }

    public PatientsContext(DbContextOptions<PatientsContext> options) : base(options)
    {
        Database.Migrate();
    }
}
public class PatientsContextFactory : IDesignTimeDbContextFactory<PatientsContext>
{
    public PatientsContext CreateDbContext(params string[] args)
    {
        string DbPath = $"{AppDomain.CurrentDomain.BaseDirectory}{System.IO.Path.DirectorySeparatorChar}patientReferrals.db";

        return new(
            new DbContextOptionsBuilder<PatientsContext>().
            UseSqlite($"Data Source={DbPath}").
            Options);
    }
}
