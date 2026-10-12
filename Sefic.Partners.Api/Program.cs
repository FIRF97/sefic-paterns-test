using Sefic.Partners.Api;
using Sefic.Partners.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 1. Kestrel sem cabeçalho do servidor
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

// 2. Serviços da Aplicação
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCustomServices(builder.Configuration);

var app = builder.Build();

// 3. Middleware Pipeline
app.UseMiddleware<GlobalExceptionMiddleware>();

// Cabeçalhos de Segurança
app.Use(async (context, next) =>
{
    context.Response.Headers.Remove("Server");
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

// Swagger sempre acessível para testes e documentação
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sefic Partners API v1");
    c.RoutePrefix = "swagger";
});

app.UseRouting();
app.UseCors("DefaultCors");

// Redirecionar rota raiz diretamente para o Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapControllers();

app.Run();

// Tornar a classe Program pública para WebApplicationFactory nos testes de integração
public partial class Program { }
