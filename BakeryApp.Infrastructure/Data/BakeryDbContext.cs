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
    
    // ** NEW MISSING DbSets **
    public DbSet<CommissionRule> CommissionRules => Set<CommissionRule>();
    public DbSet<ResellerSale> ResellerSales => Set<ResellerSale>();
    public DbSet<CommissionLedgerEntry> CommissionLedgerEntries => Set<CommissionLedgerEntry>();
    public DbSet<DeliveryAssignment> DeliveryAssignments => Set<DeliveryAssignment>();
    public DbSet<BuyerOrder> BuyerOrders => Set<BuyerOrder>();
    public DbSet<ResellerStockRequest> ResellerStockRequests => Set<ResellerStockRequest>();
    public DbSet<CashCollection> CashCollections => Set<CashCollection>();
    public DbSet<Deposit> Deposits => Set<Deposit>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ResellerApplication> ResellerApplications => Set<ResellerApplication>();
    public DbSet<DeliveryEarning> DeliveryEarnings => Set<DeliveryEarning>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<EmployeeId>().HasIndex(e => e.Code).IsUnique();

        modelBuilder.Entity<EmployeeId>()
            .HasOne(e => e.Person)
            .WithMany(p => p.EmployeeIds)
            .HasForeignKey(e => e.PersonId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EmployeeId>()
            .HasOne(e => e.Residence)
            .WithMany(r => r.AssignedResellers)
            .HasForeignKey(e => e.ResidenceId)
            .OnDelete(DeleteBehavior.SetNull);

        // Enforce that a ResellerSale must always have a CommissionRule (Rule 29)
        modelBuilder.Entity<ResellerSale>()
            .HasOne(s => s.CommissionRule)
            .WithMany()
            .HasForeignKey(s => s.CommissionRuleId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a rule if sales reference it
    }
}
