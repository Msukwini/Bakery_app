using System.Net;
using System.Net.Http.Json;
using BakeryApp.Api.Controllers;
using BakeryApp.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BakeryApp.Tests;

public class ResellerAndResidenceIntegrationTests : IClassFixture<BakeryWebApplicationFactory>
{
    private readonly BakeryWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ResellerAndResidenceIntegrationTests(BakeryWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ResidenceAndReseller_FullLifecycle_Succeeds()
    {
        // 1. Create Residence
        var createResidenceReq = new CreateResidenceRequest(
            Name: "West End Community",
            Address: "456 West Blvd",
            EstimatedPopulation: 800,
            MaxResellerCapacity: 1
        );

        var residenceResp = await _client.PostAsJsonAsync("/api/residences", createResidenceReq);
        Assert.Equal(HttpStatusCode.Created, residenceResp.StatusCode);

        var residence = await residenceResp.Content.ReadFromJsonAsync<ResidenceResponse>();
        Assert.NotNull(residence);

        // 2. Onboard 1st Reseller (Success)
        var onboardReq1 = new OnboardResellerRequest(
            FirstName: "Mark",
            LastName: "Taylor",
            Email: "mark.taylor@example.com",
            PhoneNumber: "555-9000",
            ResidenceId: residence.Id
        );

        var resellerResp1 = await _client.PostAsJsonAsync("/api/resellers", onboardReq1);
        Assert.Equal(HttpStatusCode.Created, resellerResp1.StatusCode);

        // 3. Onboard 2nd Reseller (Fails due to MaxResellerCapacity = 1)
        var onboardReq2 = new OnboardResellerRequest(
            FirstName: "Sarah",
            LastName: "Connor",
            Email: "sarah.connor@example.com",
            PhoneNumber: "555-9001",
            ResidenceId: residence.Id
        );

        var resellerResp2 = await _client.PostAsJsonAsync("/api/resellers", onboardReq2);
        Assert.Equal(HttpStatusCode.BadRequest, resellerResp2.StatusCode);
    }

    private record ResidenceResponse(Guid Id, string Name, string Address, int EstimatedPopulation, int MaxResellerCapacity);
}