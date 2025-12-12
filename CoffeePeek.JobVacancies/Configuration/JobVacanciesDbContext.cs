using CoffeePeek.JobVacancies.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.JobVacancies.Configuration;

public class JobVacanciesDbContext(DbContextOptions<JobVacanciesDbContext> options) : DbContext(options)
{
    public virtual DbSet<JobVacancy> JobVacancies { get; set; }
    public virtual DbSet<CityMap> Cities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobVacancy>()
            .HasIndex(x => x.ExternalId).IsUnique();

        modelBuilder.Entity<CityMap>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UpdatedAt).HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
        });
    }
}