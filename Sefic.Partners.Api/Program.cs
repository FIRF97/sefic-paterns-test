using Sefic.Partners.Api;
using Sefic.Partners.Api.Helper;
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

app.UseCors("DefaultCors");

// Compatibilidade do Swagger UI com OpenAPI (reescreve 3.0.4 para 3.0.1 suportado pelo regex do Swagger UI)
app.Use(async (context, next) =>
{
    if (context.Request.Path.Value != null && context.Request.Path.Value.EndsWith("swagger.json", StringComparison.OrdinalIgnoreCase))
    {
        var originalBody = context.Response.Body;
        using var memStream = new MemoryStream();
        context.Response.Body = memStream;

        await next();

        memStream.Position = 0;
        using var reader = new StreamReader(memStream);
        string responseBody = await reader.ReadToEndAsync();

        responseBody = responseBody.Replace("\"openapi\": \"3.0.4\"", "\"openapi\": \"3.0.1\"")
                                   .Replace("\"openapi\":\"3.0.4\"", "\"openapi\":\"3.0.1\"");

        context.Response.Body = originalBody;
        var bytes = System.Text.Encoding.UTF8.GetBytes(responseBody);
        context.Response.ContentLength = bytes.Length;
        await context.Response.Body.WriteAsync(bytes);
        return;
    }

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

app.MapGet("/", () => Results.Content("SEFIC Partners Test"));

app.MapControllers();

app.Run();


public partial class Program { }
