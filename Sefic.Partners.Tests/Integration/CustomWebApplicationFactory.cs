using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sefic.Partners.Data.Context;

namespace Sefic.Partners.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public CustomWebApplicationFactory()
    {
        var testDbName = $"SeficPartnersTest_{Guid.NewGuid():N}";
        _connectionString = $"Server=(localdb)\\mssqllocaldb;Database={testDbName};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remover registro anterior do DbContext
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PartnersDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Registrar com o banco de testes do SQL Server LocalDB
            services.AddDbContext<PartnersDbContext>(options =>
            {
                options.UseSqlServer(_connectionString, sql =>
                {
                    sql.EnableRetryOnFailure();
                    sql.MigrationsAssembly("Sefic.Partners.Shared");
                });
            });

            // Criar banco e tabelas usando EnsureCreated (sem precisar de migrações geradas)
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PartnersDbContext>();
            db.Database.EnsureCreated();
        });
    }

    public PartnersDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PartnersDbContext>()
            .UseSqlServer(_connectionString)
            .Options;

        return new PartnersDbContext(options);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            try
            {
                using var db = CreateDbContext();
                db.Database.EnsureDeleted();
            }
            catch
            {
                // Ignorar erros ao deletar banco temporário
            }
        }
        base.Dispose(disposing);
    }
}
