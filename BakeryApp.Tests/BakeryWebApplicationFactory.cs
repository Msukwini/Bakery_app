using System.Data.Common;
using BakeryApp.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BakeryApp.Tests;

public class BakeryWebApplicationFactory : WebApplicationFactory<Program>
{
    private DbConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 1. Remove existing DbContextOptions registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<BakeryDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // 2. Keep an open in-memory SQLite connection alive for the test lifetime
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // 3. Register DbContext using the persistent open connection
            services.AddDbContext<BakeryDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            // 4. Ensure schema and seeded data are created in the SQLite database
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BakeryDbContext>();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}