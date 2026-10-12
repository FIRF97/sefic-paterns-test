using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Sefic.Partners.Core;
using Sefic.Partners.Data;
using Sefic.Partners.Data.Context;
using Sefic.Partners.Services;
using Sefic.Partners.Services.Interface;
using Sefic.Partners.Shared;
using Sefic.Partners.Shared.Interface;
using Sefic.Partners.Shared.Models;

namespace Sefic.Partners.Api;

public static class ServiceExtensions
{
    public static void AddCustomServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Database Context configurado para apontar as migrações para Shared
        services.AddDbContext<PartnersDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? "Server=(localdb)\\mssqllocaldb;Database=SeficPartnersDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

            options.UseSqlServer(connectionString, sql =>
            {
                sql.CommandTimeout(180);
                sql.EnableRetryOnFailure();
                // Migrações localizadas no assembly Shared, conforme solicitado expressamente
                sql.MigrationsAssembly("Sefic.Partners.Shared");
            });
        });

        // 2. Camada de Dados e Repositórios
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPagedResponseBuilder, PagedResponseBuilder>();

        // 3. Camada de Serviços de Domínio
        services.AddScoped<IPartnerService, PartnerService>();

        // 4. UriService com suporte a fallback
        services.AddHttpContextAccessor();
        services.AddScoped<IUriService>(sp =>
        {
            var accessor = sp.GetRequiredService<IHttpContextAccessor>();
            var request = accessor.HttpContext?.Request;
            var uri = request != null
                ? string.Concat(request.Scheme, "://", request.Host.ToUriComponent())
                : "http://localhost:8080";

            return new UriService(uri);
        });

        // 5. Formato Consistente de Erros de Modelo (400 Bad Request)
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .SelectMany(e => e.Value!.Errors.Select(err => 
                        !string.IsNullOrWhiteSpace(err.ErrorMessage) ? err.ErrorMessage : "Valor inválido fornecido."))
                    .ToList();

                var errorResponse = new ErrorResponse(
                    StatusCodes.Status400BadRequest,
                    "Requisição inválida. Verifique os dados fornecidos.",
                    errors
                );

                return new BadRequestObjectResult(errorResponse);
            };
        });

        // 6. CORS
        services.AddCors(options =>
        {
            options.AddPolicy("DefaultCors", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        // 7. Swagger
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Sefic Partners API",
                Version = "v1",
                Description = "API de Gestão de Clientes e Fornecedores (Fase 2 - Sefic Web)"
            });
        });
    }
}
