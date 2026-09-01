using BakeryApp.Core.Entities;
using BakeryApp.Core.Enums;

namespace BakeryApp.Infrastructure.Data;

public static class DbInitializer
{
    public static void Initialize(BakeryDbContext context)
    {
        // Ensure database is created
        context.Database.EnsureCreated();

        // Stop if data already exists
        if (context.Products.Any()) return;

        // 1. Seed Residences
        var residenceA = new Residence
        {
            Name = "Sunset Heights Residence",
            Address = "124 Hillside Avenue",
            EstimatedPopulation = 450,
            MaxResellerCapacity = 2
        };

        var residenceB = new Residence
        {
            Name = "Green Valley Estates",
            Address = "88 Meadow Lane",
            EstimatedPopulation = 800,
            MaxResellerCapacity = 4
        };

        context.Residences.AddRange(residenceA, residenceB);

        // 2. Seed Products and Variants
        var creamBucket = new Product
        {
            Name = "Vanilla Cream Bucket",
            Description = "Fresh bulk bakery vanilla cream",
            IsActive = true,
            Variants = new List<ProductVariant>
            {
                new ProductVariant { SizeName = "5L Bucket", UnitPrice = 15.00m, BaseCommissionAmount = 2.00m },
                new ProductVariant { SizeName = "10L Bucket", UnitPrice = 28.00m, BaseCommissionAmount = 4.00m }
            }
        };

        var breadDough = new Product
        {
            Name = "Pre-Mix Bread Dough",
            Description = "Ready-to-bake dough batch",
            IsActive = true,
            Variants = new List<ProductVariant>
            {
                new ProductVariant { SizeName = "10kg Bag", UnitPrice = 22.00m, BaseCommissionAmount = 3.00m }
            }
        };

        context.Products.AddRange(creamBucket, breadDough);
        context.SaveChanges(); // Save to generate entity IDs

        // 3. Seed Reseller Person & Employee ID
        var person = new Person
        {
            FirstName = "Sarah",
            LastName = "Jenkins",
            Email = "sarah.j@bakerydist.com",
            PhoneNumber = "+15550192834"
        };

        var resellerEmployee = new EmployeeId
        {
            Code = "RES-1001",
            RoleType = EmployeeRoleType.Reseller,
            ResidenceId = residenceA.Id,
            Person = person
        };

        context.Persons.Add(person);
        context.EmployeeIds.Add(resellerEmployee);

        // 4. Seed Initial Production Stock Ledger Entry
        var variant5L = creamBucket.Variants.First(v => v.SizeName == "5L Bucket");
        var initialStock = new InventoryLedgerEntry
        {
            ProductVariantId = variant5L.Id,
            TransactionType = InventoryTransactionType.ProductionBatch,
            Quantity = 100,
            ReferenceNote = "Initial Batch Production",
            Timestamp = DateTime.UtcNow
        };

        context.InventoryLedgerEntries.Add(initialStock);
        context.SaveChanges();
    }
}