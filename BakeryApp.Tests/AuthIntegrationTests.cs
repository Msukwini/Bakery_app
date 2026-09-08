using System.Net;
using System.Net.Http.Json;
using BakeryApp.Api.Controllers;
using BakeryApp.Core.Entities;
using BakeryApp.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BakeryApp.Tests;

public class AuthIntegrationTests : IClassFixture<BakeryWebApplicationFactory>
{
    private readonly BakeryWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthIntegrationTests(BakeryWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ReturnsOkAndToken_WhenCredentialsAreValid()
    {
        var email = $"user.{Guid.NewGuid().ToString()[..4]}@example.com";
        var code = $"EMP-{Guid.NewGuid().ToString()[..4]}";

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<BakeryDbContext>();

            var person = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = "Alice",
                LastName = "Staff",
                Email = email,
                PhoneNumber = "555-1234",
                CreatedAt = DateTime.UtcNow
            };

            var employee = new EmployeeId
            {
                Id = Guid.NewGuid(),
                Code = code,
                RoleType = RoleType.BakeryStaff,
                IsActive = true,
                AssignedAt = DateTime.UtcNow,
                PersonId = person.Id
            };

            dbContext.Persons.Add(person);
            dbContext.EmployeeIds.Add(employee);
            await dbContext.SaveChangesAsync();
        }

        var loginRequest = new LoginRequest(email, code);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.Equal("BakeryStaff", result.Role);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenCredentialsAreInvalid()
    {
        var loginRequest = new LoginRequest("nonexistent@example.com", "INVALID-CODE");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private record LoginResponse(string Token, Guid EmployeeId, string Role);
}