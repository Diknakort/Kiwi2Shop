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
var identityApi = builder.AddProject<Projects.Kiwi2Shop_identity>("identity-api")
    .WithReference(identityDb);

//// Agregar el proyecto React
//var frontend = builder.AddNpmApp("kiwishop-web", "../kiwishop-web")
//    .WithReference(identityApi)
//    .WithHttpEndpoint(port: 5173, env: "PORT")
//    .WithExternalHttpEndpoints()
//    .PublishAsDockerFile();


builder.Build().Run();
