using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

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
    Vault v;

    public PatientsContextFactory(Vault v)
    {
        this.v = v;
    }

    public PatientsContext CreateDbContext(params string[] args)
    {
        return new(
            new DbContextOptionsBuilder<PatientsContext>().
            UseSqlite($"Data Source={v.DatabasePath}").
            Options);
    }
}
