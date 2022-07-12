using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;

namespace IntakeTrackerApp.DataManagement;
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
    public Vault? v { get; init; }

    public PatientsContextFactory()
    {
    }

    public PatientsContext CreateDbContext(params string[] args)
    {
        var p = v?.DatabasePath ?? Path.Join(AppDomain.CurrentDomain.BaseDirectory, "patientReferrals.db");


        return new(
            new DbContextOptionsBuilder<PatientsContext>().
            UseSqlite($"Data Source={p}").
            Options);
    }
}
