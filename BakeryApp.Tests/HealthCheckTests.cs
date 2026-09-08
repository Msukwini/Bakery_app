using System.Net;
using Xunit;

namespace BakeryApp.Tests;

public class HealthCheckTests : IClassFixture<BakeryWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthCheckTests(BakeryWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("Healthy", content);
    }
}