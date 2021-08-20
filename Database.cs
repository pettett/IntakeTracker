using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.IO;
using Microsoft.EntityFrameworkCore.Design;

namespace IntakeTrackerApp;

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
       var DbPath = $"{ AppDomain.CurrentDomain.BaseDirectory}{System.IO.Path.DirectorySeparatorChar}patientReferrals.db"!;
        var optionsBuilder = new DbContextOptionsBuilder<PatientsContext>();
        optionsBuilder.UseSqlite($"Data Source={DbPath}");

        return new PatientsContext(optionsBuilder.Options);
    }
}
