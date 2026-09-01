using System.Net;
using System.Net.Http.Json;
using BakeryApp.Api;
using BakeryApp.Infrastructure.Data;
using BakeryApp.Core.Entities;
using BakeryApp.Core.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class BakeryWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration to use an isolated SQLite in-memory/test DB
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<BakeryDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<BakeryDbContext>(options =>
            {
                options.UseSqlite("DataSource=:memory:");
            });

            // Build the service provider and initialize the database schema & seed data
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BakeryDbContext>();
            db.Database.OpenConnection();
            db.Database.EnsureCreated();
            
            // Seed a test product variant and reseller if not already seeded
            SeedTestData(db);
        });
    }

    private static void SeedTestData(BakeryDbContext db)
    {
        if (!db.ProductVariants.Any())
        {
            var product = new Product { Id = Guid.NewGuid(), Name = "Test Bread", Description = "Test", IsActive = true };
            var variant = new ProductVariant 
            { 
                Id = Guid.Parse("61d686ad-cc18-42f3-921b-171c4ce5cf92"), 
                ProductId = product.Id, 
                SizeName = "Standard", 
                UnitPrice = 20.0m, 
                BaseCommissionAmount = 4.0m, 
                IsActive = true 
            };
            db.Products.Add(product);
            db.ProductVariants.Add(variant);

            var person = new Person { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", Email = "jane@example.com", PhoneNumber = "1234567890" };
            var residence = new Residence { Id = Guid.NewGuid(), Name = "Test Estate", Address = "123 Test St", EstimatedPopulation = 100, MaxResellerCapacity = 5 };
            var employeeId = new EmployeeId 
            { 
                Id = Guid.Parse("E5BD7D5D-B1BA-4888-BF97-8F713C9C88A6"), 
                Code = "RES-1001", 
                PersonId = person.Id, 
                ResidenceId = residence.Id, 
                RoleType = EmployeeRoleType.Reseller, 
                IsActive = true 
            };

            db.Persons.Add(person);
            db.Residences.Add(residence);
            db.EmployeeIds.Add(employeeId);
            db.SaveChanges();
        }
    }
}

public class SalesAndInventoryIntegrationTests : IClassFixture<BakeryWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly Guid _variantId = Guid.Parse("61d686ad-cc18-42f3-921b-171c4ce5cf92");
    private readonly Guid _resellerId = Guid.Parse("E5BD7D5D-B1BA-4888-BF97-8F713C9C88A6");

    public SalesAndInventoryIntegrationTests(BakeryWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RecordProductionBatch_ShouldIncreaseStock_AndReturnSuccess()
    {
        // Arrange
        var request = new { ProductVariantId = _variantId, Quantity = 100, ReferenceNote = "Integration Test Batch" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/inventory/production-batch", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ProductionBatchResponse>();
        Assert.NotNull(result);
        Assert.Equal(100, result.QuantityAdded);
    }

    [Fact]
    public async Task DispatchAndCommissionWorkflow_ShouldCalculateCommissionsCorrectly()
    {
        // 1. Ensure stock is available via Production Batch
        await _client.PostAsJsonAsync("/api/inventory/production-batch", new { ProductVariantId = _variantId, Quantity = 50 });

        // 2. Dispatch items to reseller
        var dispatchRequest = new 
        { 
            ResellerEmployeeId = _resellerId, 
            ProductVariantId = _variantId, 
            Quantity = 15 
        };

        var dispatchResponse = await _client.PostAsJsonAsync("/api/sales/dispatch", dispatchRequest);
        Assert.Equal(HttpStatusCode.OK, dispatchResponse.StatusCode);

        // 3. Verify commission summary endpoint
        var commissionResponse = await _client.GetAsync($"/api/sales/commissions/{_resellerId}");
        Assert.Equal(HttpStatusCode.OK, commissionResponse.StatusCode);

        var summary = await commissionResponse.Content.ReadFromJsonAsync<ResellerCommissionSummaryDto>();
        Assert.NotNull(summary);
        Assert.True(summary.TotalItemsDispatched >= 15);
        Assert.True(summary.TotalCommissionEarned > 0);
    }

    private record ProductionBatchResponse(string Message, Guid EntryId, int QuantityAdded);
    private record ResellerCommissionSummaryDto(string ResellerCode, string ResellerName, int TotalItemsDispatched, decimal TotalCommissionEarned);
}