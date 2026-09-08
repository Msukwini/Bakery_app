using BakeryApp.Core.Entities;
using BakeryApp.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace BakeryApp.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(BakeryDbContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();

        // 1. Seed Admin Account
        if (!await dbContext.Persons.AnyAsync(p => p.Email == "admin@bakeryapp.com"))
        {
            var adminPerson = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = "System",
                LastName = "Administrator",
                Email = "admin@bakeryapp.com",
                PhoneNumber = "555-0000",
                CreatedAt = DateTime.UtcNow
            };

            var adminEmployee = new EmployeeId
            {
                Id = Guid.NewGuid(),
                Code = "ADM-001",
                RoleType = EmployeeRoleType.Admin,
                IsActive = true,
                AssignedAt = DateTime.UtcNow,
                PersonId = adminPerson.Id
            };

            dbContext.Persons.Add(adminPerson);
            dbContext.EmployeeIds.Add(adminEmployee);
        }

        // 2. Seed Sample Products & Variants
        if (!await dbContext.Products.AnyAsync())
        {
            var sourdough = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Artisan Sourdough",
                Description = "Slow-fermented traditional sourdough bread",
                IsActive = true
            };

            var sourdoughSingle = new ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = sourdough.Id,
                SizeName = "Single Loaf",
                UnitPrice = 4.50m,
                IsActive = true
            };

            var sourdoughPack = new ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = sourdough.Id,
                SizeName = "Family Pack (3 Loaves)",
                UnitPrice = 12.00m,
                IsActive = true
            };

            var croissant = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Butter Croissant",
                Description = "Flaky French butter croissants",
                IsActive = true
            };

            var croissantPack = new ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = croissant.Id,
                SizeName = "Pack of 4",
                UnitPrice = 6.00m,
                IsActive = true
            };

            dbContext.Products.AddRange(sourdough, croissant);
            dbContext.ProductVariants.AddRange(sourdoughSingle, sourdoughPack, croissantPack);
        }

        // 3. Seed Commission Rules for those variants (if none exist)
        if (!await dbContext.CommissionRules.AnyAsync())
        {
            // Get the variants we just seeded (or find them by name)
            var variants = await dbContext.ProductVariants.ToListAsync();
            foreach (var variant in variants)
            {
                // For simplicity, set a rule of 10% of UnitPrice as commission per unit
                var rule = new CommissionRule
                {
                    Id = Guid.NewGuid(),
                    ProductVariantId = variant.Id,
                    RatePerUnit = variant.UnitPrice * 0.10m, // e.g., 0.45 for 4.50 loaf
                    EffectiveDate = DateTime.UtcNow,
                    IsActive = true
                };
                dbContext.CommissionRules.Add(rule);
            }
        }

        await dbContext.SaveChangesAsync();
    }
}
