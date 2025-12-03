var builder = DistributedApplication.CreateBuilder(args);

var maildev = builder
    .AddContainer("maildev", "maildev/maildev:latest") // nombre e imagen
    .WithLifetime(ContainerLifetime.Persistent) // Mantiene datos entre reinicios
    .WithHttpEndpoint(port: 1080, targetPort: 1080, name: "web") // Puerto para la interfaz web de MailDev (TargetPort especificado)
    .WithEndpoint(port: 1025, targetPort: 1025, name: "smtp"); // Puerto SMTP para enviar correos (TargetPort especificado)

// 1. Agregar el servidor PostgreSQL (usará la imagen oficial de Docker)
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin() // Habilita pgAdmin
    .WithLifetime(ContainerLifetime.Persistent) 
    .WithDataVolume("postgres");

var identityDb = postgres.AddDatabase("identitydb");

// 2. Agregar RabbitMQ con el plugin de gestión habilitado (usará la imagen oficial de Docker) necesita el using Aspire.Hosting.RabbitMQ; para mensajeria
var rabbit = builder
    .AddRabbitMQ("rabbitmq")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("rabbitmq-data")
    .WithManagementPlugin();

// 3. Agregar Redis (usará la imagen oficial de Docker) necesita el using Aspire.Hosting; para el metodo AddRedis para cache y sesiones
var redis = builder.AddRedis("redis")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("redis-data-identity")
    .WithRedisInsight();

// Agregar la API de Identity
var identityApi = builder.AddProject<Projects.Kiwi2Shop_Identity>("identity-api")
    .WaitFor(identityDb)
    .WaitFor(rabbit)
    .WithReference(rabbit)
    .WithReference(identityDb);

// Agregar la API de GateWay
var gatewayApi = builder.AddProject<Projects.Kiwi2Shop_GateWay>("gateway-api")
    .WaitFor(identityApi)
    .WaitFor(redis)
    .WithReference(identityApi)
    .WithReference(redis);

//añadir referencia al servicio ORDERS y a PRODUCTS cuando existan
var productsDb = postgres.AddDatabase("ProductsDb");

var productsApi = builder.AddProject<Projects.Kiwi2Shop_ProductsAPI>("kiwi2shop-productsapi")
    .WithReference(productsDb);


var ordersDb = postgres.AddDatabase("OrdersDb");


var ordersApi = builder.AddProject<Projects.Kiwi2Shop_OrdersAPI>("kiwi2shop-ordersapi")
    .WithReference(ordersDb);


// ============================================
// FRONTEND - React App
// ============================================
var frontendApp = builder.AddNpmApp("kiwi2shop-frontend", "../kiwi2shop.frontend", "dev")
    .WithReference(identityApi)
    .WithEnvironment("VITE_IDENTITY_URL", identityApi.GetEndpoint("https"))
    .WithHttpEndpoint(env: "PORT") // Let Aspire assign port dynamically
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();


builder.AddProject<Projects.Kiwi2Shop_reverseProxy>("kiwi2shop-reverseproxy");


builder.AddProject<Projects.Kiwi2Shop_Notifications>("kiwi2shop-notifications")
    .WaitFor(rabbit)
    .WithReference(rabbit);


builder.AddProject<Projects.Kiwi2Shop_ProductsAPI>("kiwi2shop-productsapi")
    .WithReference(productsApi);


builder.AddProject<Projects.Kiwi2Shop_OrdersAPI>("kiwi2shop-ordersapi")
    .WithReference(ordersApi);


builder.Build().Run();