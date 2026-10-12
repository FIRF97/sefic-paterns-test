using Microsoft.EntityFrameworkCore;
using Sefic.Partners.Shared.Enums;
using Sefic.Partners.Shared.Model;

namespace Sefic.Partners.Data.Context;

public class PartnersDbContext : DbContext
{
    public DbSet<Partner> Partners => Set<Partner>();

    public PartnersDbContext(DbContextOptions<PartnersDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Partner>(entity =>
        {
            entity.ToTable("Partners");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(p => p.TaxNumber)
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false);

            // NIF é único em todo o cadastro, incluindo entidades inativas (regra garantida no SQL Server)
            entity.HasIndex(p => p.TaxNumber)
                .IsUnique()
                .HasDatabaseName("IX_Partners_TaxNumber_Unique");

            entity.Property(p => p.Type)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(p => p.Email)
                .HasMaxLength(255)
                .IsRequired(false);

            entity.Property(p => p.CreditLimit)
                .HasPrecision(18, 2)
                .HasColumnType("decimal(18,2)");

            entity.Property(p => p.IsActive)
                .IsRequired();

            entity.Property(p => p.CreatedAt)
                .IsRequired();

            entity.Property(p => p.UpdatedAt)
                .IsRequired();

            // Optimistic Concurrency Token via SQL Server rowversion
            entity.Property(p => p.Version)
                .IsRowVersion()
                .IsRequired();
        });
    }
}
