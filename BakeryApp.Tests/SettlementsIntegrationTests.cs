using System.Net;
using System.Net.Http.Json;
using BakeryApp.Api.Controllers;
using BakeryApp.Core.Entities;
using BakeryApp.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BakeryApp.Tests;

public class SettlementsIntegrationTests : IClassFixture<BakeryWebApplicationFactory>
{
    private readonly BakeryWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public SettlementsIntegrationTests(BakeryWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SettlementPayout_UpdatesPendingBalanceCorrectly()
    {
        var resellerId = Guid.NewGuid();
        var productVariantId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<BakeryDbContext>();

            var person = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = "David",
                LastName = "Miller",
                Email = "david.m@example.com",
                PhoneNumber = "555-4321",
                CreatedAt = DateTime.UtcNow
            };

            var reseller = new EmployeeId
            {
                Id = resellerId,
                Code = "RES-SETTLE-1",
                RoleType = RoleType.Reseller,
                IsActive = true,
                AssignedAt = DateTime.UtcNow,
                PersonId = person.Id
            };

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Cinnamon Roll",
                Description = "Fresh cinnamon roll",
                IsActive = true
            };

            var variant = new ProductVariant
            {
                Id = productVariantId,
                ProductId = product.Id,
                SizeName = "Single",
                UnitPrice = 2.00m,
                BaseCommissionAmount = 0.50m,
                IsActive = true
            };

            dbContext.Persons.Add(person);
            dbContext.EmployeeIds.Add(reseller);
            dbContext.Products.Add(product);
            dbContext.ProductVariants.Add(variant);
            await dbContext.SaveChangesAsync();
        }

        // 1. Dispatch 20 units -> Earns 20 * 0.50 = 10.00 commission
        var dispatchReq = new DispatchOrderRequest(resellerId, productVariantId, 20);
        var dispatchResp = await _client.PostAsJsonAsync("/api/sales/dispatch", dispatchReq);
        Assert.Equal(HttpStatusCode.OK, dispatchResp.StatusCode);

        // 2. Query Balance (Expected Earned: 10.00, Paid: 0.00, Pending: 10.00)
        var balanceResp1 = await _client.GetAsync($"/api/settlements/balance/{resellerId}");
        Assert.Equal(HttpStatusCode.OK, balanceResp1.StatusCode);
        var balance1 = await balanceResp1.Content.ReadFromJsonAsync<SettlementBalanceResponse>();
        Assert.NotNull(balance1);
        Assert.Equal(10.00m, balance1.PendingBalance);

        // 3. Record Payout of 6.00
        var payoutReq = new RecordPayoutRequest(resellerId, 6.00m, "BANK-REF-9988");
        var payoutResp = await _client.PostAsJsonAsync("/api/settlements/payout", payoutReq);
        Assert.Equal(HttpStatusCode.OK, payoutResp.StatusCode);

        // 4. Query Balance (Expected Earned: 10.00, Paid: 6.00, Pending: 4.00)
        var balanceResp2 = await _client.GetAsync($"/api/settlements/balance/{resellerId}");
        Assert.Equal(HttpStatusCode.OK, balanceResp2.StatusCode);
        var balance2 = await balanceResp2.Content.ReadFromJsonAsync<SettlementBalanceResponse>();
        Assert.NotNull(balance2);
        Assert.Equal(6.00m, balance2.TotalPaidCommission);
        Assert.Equal(4.00m, balance2.PendingBalance);
    }

    private record SettlementBalanceResponse(
        Guid ResellerId,
        decimal TotalEarnedCommission,
        decimal TotalPaidCommission,
        decimal PendingBalance
    );
}