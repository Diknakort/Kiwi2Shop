using Kiwi2Shop.identity.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// endpoints API explorer
builder.Services.AddEndpointsApiExplorer();
// Agregar conexión a PostgreSQL desde Aspire
builder.AddNpgsqlDbContext<ApplicationDbContext>("identitydb");

// Configurar CORS para React
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseHttpsRedirection();
app.UseCors();
app.UseRouting();

app.UseAuthentication();



app.UseAuthorization();

app.MapControllers();

app.Run();
