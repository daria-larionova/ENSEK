using ENSEK.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ENSEK.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<MeterReading> MeterReadings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId);
            entity.Property(e => e.AccountId).ValueGeneratedNever();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<MeterReading>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MeterReadValue).IsRequired().HasMaxLength(5);
            entity.Property(e => e.MeterReadingDateTime).IsRequired();
            entity.HasIndex(e => new { e.AccountId, e.MeterReadingDateTime }).IsUnique();
            entity.HasOne(e => e.Account)
                  .WithMany(a => a.MeterReadings)
                  .HasForeignKey(e => e.AccountId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
