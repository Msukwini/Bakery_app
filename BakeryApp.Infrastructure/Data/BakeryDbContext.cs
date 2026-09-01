namespace BakeryApp.Infrastructure.Data;

using BakeryApp.Core.Entities;
using Microsoft.EntityFrameworkCore;

public class BakeryDbContext : DbContext
{
    public BakeryDbContext(DbContextOptions<BakeryDbContext> options) : base(options) { }

    public DbSet<Person> Persons => Set<Person>();
    public DbSet<EmployeeId> EmployeeIds => Set<EmployeeId>();
    public DbSet<Residence> Residences => Set<Residence>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<InventoryLedgerEntry> InventoryLedgerEntries => Set<InventoryLedgerEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Enforce unique employee badge/code
        modelBuilder.Entity<EmployeeId>()
            .HasIndex(e => e.Code)
            .IsUnique();

        // Person to EmployeeIds (One-to-Many)
        modelBuilder.Entity<EmployeeId>()
            .HasOne(e => e.Person)
            .WithMany(p => p.EmployeeIds)
            .HasForeignKey(e => e.PersonId)
            .OnDelete(DeleteBehavior.Cascade);

        // Residence to EmployeeIds (One-to-Many)
        modelBuilder.Entity<EmployeeId>()
            .HasOne(e => e.Residence)
            .WithMany(r => r.AssignedResellers)
            .HasForeignKey(e => e.ResidenceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}