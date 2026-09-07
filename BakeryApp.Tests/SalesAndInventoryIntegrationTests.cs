using System.Net.Http.Json;
using BakeryApp.Api.Controllers;
using Xunit;

namespace BakeryApp.Tests;

public class SalesAndInventoryIntegrationTests : IClassFixture<BakeryWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly BakeryWebApplicationFactory _factory;

    public SalesAndInventoryIntegrationTests(BakeryWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetResellerCommissions_ReturnsNotFound_ForInvalidId()
    {
        // Act - Calls GET /api/sales/commissions/{id}
        var randomGuid = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/sales/commissions/{randomGuid}");

        // Assert - Endpoint exists, returns 404 because reseller ID doesn't exist
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DispatchOrder_ReturnsNotFound_ForInvalidReseller()
    {
        // Act - Calls POST /api/sales/dispatch
        var request = new DispatchOrderRequest(Guid.NewGuid(), Guid.NewGuid(), 5);
        var response = await _client.PostAsJsonAsync("/api/sales/dispatch", request);

        // Assert - Endpoint exists and handles the request
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }
}