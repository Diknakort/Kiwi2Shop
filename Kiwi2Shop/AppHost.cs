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
var frontendApp = builder.AddNpmApp("kiwi2shop-frontend", "../kiwi2shop.frontend", "dev")
    .WithReference(identityApi)
    .WithEnvironment("VITE_IDENTITY_URL", identityApi.GetEndpoint("https"))
    .WithHttpEndpoint(env: "PORT") // Let Aspire assign port dynamically
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();


builder.Build().Run();




//var builder = DistributedApplication.CreateBuilder(args);

//builder.AddProject<Projects.Kiwi2Shop_identity>("kiwi2shop-identity");

//builder.Build().Run();

// Program.cs en el proyecto AppHost

// cambios YARP no funciona
//////using Aspire.Hosting;

//////var builder = DistributedApplication.CreateBuilder(args);


//////// 1. Agregar el servidor PostgreSQL (usará la imagen oficial de Docker)
//////var postgres = builder.AddPostgres("postgres")
//////                      .WithImage("postgres:latest")                   // Imagen oficial
//////                      .WithEnvironment("POSTGRES_PASSWORD", "htVepm3{tDKv.p4H4wP9cF") // Contraseña del superusuario
//////                      .WithLifetime(ContainerLifetime.Persistent)
//////                      .WithDataVolume("identity-kiwi2")
//////                      ;     // Mantiene datos entre reinicios
//////                                                                       //.WithPort(5432, 5432);                          // Puerto mapeado (opcional)

//////var identityDb = postgres.AddDatabase("identitydb");

//////// Agregar la API de Identity
//////var identityApi = builder.AddProject<Projects.Kiwi2Shop_Identity>("identity-api")
//////    .WaitFor(identityDb)
//////    .WithReference(identityDb);
//////    //.WithHttpEndpoint(port: 5001); // Puerto fijo para YARP

//////// Gateway (YARP)
//////var gateway = builder.AddProject<Projects.Kiwi2Shop_GateWay>("gateway")
//////    .WithReference(identityApi)
//////    //.WithHttpEndpoint(port: 5000, name: "http")
//////    .WithExternalHttpEndpoints();

//////// React Frontend
//////var frontend = builder.AddNpmApp("kiwishop-web", "../kiwishop-web")
//////    .WithReference(gateway) // Conectar al Gateway en lugar de directamente a la API
//////    .WithEnvironment("VITE_IDENTITY_URL", identityApi.GetEndpoint("https"))
//////    .WithHttpEndpoint(port: 5173, env: "PORT")
//////    .WithExternalHttpEndpoints()
//////    .PublishAsDockerFile();

//////builder.Build().Run();


/////  ============================================  OLD CODE  ============================================

//// ============================================
//// FRONTEND - React App
//// ============================================
//var frontendApp = builder.AddNpmApp("kiwi2shop-frontend", "../kiwi2shop.frontend","dev")
//    .WithReference(identityApi)
//    .WithEnvironment("VITE_IDENTITY_URL", identityApi.GetEndpoint("https"))
//    .WithHttpEndpoint(env: "PORT") // Let Aspire assign port dynamically
//    .WithExternalHttpEndpoints()
//    .PublishAsDockerFile();



//builder.AddProject<Projects.Kiwi2Shop_GateWay>("kiwi2shop-gateway");


//builder.Build().Run();
