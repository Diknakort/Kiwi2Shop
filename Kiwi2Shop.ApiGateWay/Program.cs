using Kiwi2Shop.GateWay.Extensions;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Agregar Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync(
            "Demasiadas solicitudes. Por favor, intenta más tarde.",
            token);
    };
});

// Agregar YARP
builder.Services.AddYarpReverseProxy(builder.Configuration);
;

// Configurar CORS
builder.Services.AddGatewayCors();
builder.Services.AddHealthChecks();
var app = builder.Build();

// Middleware personalizado
app.MapHealthChecks("/health");
app.UseCors();
app.UseRateLimiter();

// Usar YARP
app.MapReverseProxy();

app.Run();
