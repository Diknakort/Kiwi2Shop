//var builder = DistributedApplication.CreateBuilder(args);

//builder.AddProject<Projects.Kiwi2Shop_identity>("kiwi2shop-identity");

//builder.Build().Run();

// Program.cs en el proyecto AppHost


using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);


// 1. Agregar el servidor PostgreSQL (usará la imagen oficial de Docker)
var postgres = builder.AddPostgres("postgres")
                      .WithImage("postgres:latest")                   // Imagen oficial
                      .WithEnvironment("POSTGRES_PASSWORD", "htVepm3{tDKv.p4H4wP9cF") // Contraseña del superusuario
                      .WithLifetime(ContainerLifetime.Persistent);     // Mantiene datos entre reinicios
                                                                       //.WithPort(5432, 5432);                          // Puerto mapeado (opcional)

var identityDb = postgres.AddDatabase("identitydb");

// Agregar la API de Identity
var identityApi = builder.AddProject<Projects.Kiwi2Shop_Identity>("identity-api")
    .WaitFor(identityDb)
    .WithReference(identityDb);

// ============================================
// FRONTEND - React App
// ============================================
var frontendApp = builder.AddNpmApp("kiwi2shop-frontend", "../kiwi2shop.frontend","dev")
    .WithReference(identityApi)
    .WithEnvironment("VITE_IDENTITY_URL", identityApi.GetEndpoint("https"))
    .WithHttpEndpoint(env: "PORT") // Let Aspire assign port dynamically
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();


builder.AddProject<Projects.Kiwi2Shop_apigateaway>("kiwi2shop-apigateaway");


builder.Build().Run();
