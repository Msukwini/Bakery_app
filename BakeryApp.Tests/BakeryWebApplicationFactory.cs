using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;
using BakeryApp.Infrastructure.Data;

public class BakeryWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    private DbConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<BakeryDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Fix: Use a named in-memory database so Cache=Shared correctly shares the tables across contexts
            services.AddSingleton<DbConnection>(container =>
            {
                var connection = new SqliteConnection("Data Source=BakeryTestDb;Mode=Memory;Cache=Shared");
                connection.Open(); // Keep the root connection open so the in-memory DB persists
                _connection = connection;
                return connection;
            });

            services.AddDbContext<BakeryDbContext>((container, options) =>
            {
                var connection = container.GetRequiredService<DbConnection>();
                options.UseSqlite(connection);
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Dispose();
    }
}