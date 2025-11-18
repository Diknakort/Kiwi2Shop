using System.Threading.RateLimiting;
using Kiwi2Shop.GateWay.Middleware;


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
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
builder.Services.AddHealthChecks();
var app = builder.Build();

// Middleware personalizado
app.UseMiddleware<RequestLoggingMiddleware>();
app.MapHealthChecks("/health");
app.UseCors();
app.UseRateLimiter();

// Usar YARP
app.MapReverseProxy();

app.Run();

/////// === version 1.1 de YARP with CORS === ///////
////var builder = WebApplication.CreateBuilder(args);

////// Agregar YARP
////builder.Services.AddReverseProxy()
////    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

////// Configurar CORS
////builder.Services.AddCors(options =>
////{
////    options.AddDefaultPolicy(policy =>
////    {
////        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
////              .AllowAnyHeader()
////              .AllowAnyMethod()
////              .AllowCredentials();
////    });
////});

////var app = builder.Build();

////app.UseCors();

////// Usar YARP
////app.MapReverseProxy();

////app.Run();


/////// === OLD CODE === ///////
//var builder = WebApplication.CreateBuilder(args);

//builder.AddServiceDefaults();

//// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

//var app = builder.Build();

//app.MapDefaultEndpoints();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();
