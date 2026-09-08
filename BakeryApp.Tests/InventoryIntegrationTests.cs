using System.Net;
using System.Net.Http.Json;
using BakeryApp.Api.Controllers;
using BakeryApp.Core.Entities;
using BakeryApp.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BakeryApp.Tests;

public class InventoryIntegrationTests : IClassFixture<BakeryWebApplicationFactory>
{
    private readonly BakeryWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public InventoryIntegrationTests(BakeryWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RecordBatch_And_CheckStock_UpdatesLedgerCorrectly()
    {
        var productVariantId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<BakeryDbContext>();

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Brioche Bun",
                Description = "Sweet brioche bun",
                IsActive = true
            };

            var variant = new ProductVariant
            {
                Id = productVariantId,
                ProductId = product.Id,
                SizeName = "Pack of 6",
                UnitPrice = 3.50m,
                BaseCommissionAmount = 0.40m,
                IsActive = true
            };

            dbContext.Products.Add(product);
            dbContext.ProductVariants.Add(variant);
            await dbContext.SaveChangesAsync();
        }

        // Act 1: Add Batch of 50 units
        var batchRequest = new RecordBatchRequest(productVariantId, 50, "BATCH-101");
        var batchResponse = await _client.PostAsJsonAsync("/api/inventory/batch", batchRequest);
        Assert.Equal(HttpStatusCode.OK, batchResponse.StatusCode);

        // Act 2: Write off 5 spoiled units
        var writeOffRequest = new WriteOffRequest(productVariantId, 5, "Damaged in oven");
        var writeOffResponse = await _client.PostAsJsonAsync("/api/inventory/write-off", writeOffRequest);
        Assert.Equal(HttpStatusCode.OK, writeOffResponse.StatusCode);

        // Act 3: Query current stock level (Expected: 50 - 5 = 45)
        var stockResponse = await _client.GetAsync($"/api/inventory/stock/{productVariantId}");
        Assert.Equal(HttpStatusCode.OK, stockResponse.StatusCode);

        var result = await stockResponse.Content.ReadFromJsonAsync<StockLevelResponse>();
        Assert.NotNull(result);
        Assert.Equal(45, result.CurrentStockLevel);
    }

    private record StockLevelResponse(Guid ProductVariantId, int CurrentStockLevel);
}