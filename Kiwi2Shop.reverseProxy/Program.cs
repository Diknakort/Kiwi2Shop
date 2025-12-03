using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

// Cargar configuración de ReverseProxy desde appsettings.json
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// (Opcional) middlewares antes del proxy: autenticación, logging, etc.

// Mapea el reverse proxy
app.MapReverseProxy();

app.Run();
