using BakeryApp.Core.Entities;
using BakeryApp.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace BakeryApp.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(BakeryDbContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();

        // 1. Seed Admin Account if it doesn't exist
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
                RoleType = EmployeeRoleType.Admin, // was EmployeeRoleType.Reseller - the seeded "admin" had the wrong role
                IsActive = true,
                AssignedAt = DateTime.UtcNow,
                PersonId = adminPerson.Id
            };

            dbContext.Persons.Add(adminPerson);
            dbContext.EmployeeIds.Add(adminEmployee);
        }

        // 2. Seed Sample Products & Product Variants if none exist
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
                BaseCommissionAmount = 0.50m,
                IsActive = true
            };

            var sourdoughPack = new ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = sourdough.Id,
                SizeName = "Family Pack (3 Loaves)",
                UnitPrice = 12.00m,
                BaseCommissionAmount = 1.50m,
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
                BaseCommissionAmount = 0.80m,
                IsActive = true
            };

            dbContext.Products.AddRange(sourdough, croissant);
            dbContext.ProductVariants.AddRange(sourdoughSingle, sourdoughPack, croissantPack);
        }

        await dbContext.SaveChangesAsync();
    }
}