var builder = DistributedApplication.CreateBuilder(args);

var transportUserName = builder.AddParameter("transportUserName", "guest", secret: true);
var transportPassword = builder.AddParameter("transportPassword", "guest", secret: true);

var transport = builder.AddRabbitMQ("transport", transportUserName, transportPassword)
    .WithManagementPlugin(15672)
    .WithUrlForEndpoint("management", url => url.DisplayText = "RabbitMQ Management")
    .WithLifetime(ContainerLifetime.Persistent);

transportUserName.WithParentRelationship(transport);
transportPassword.WithParentRelationship(transport);

var postgresDbServer = builder.AddPostgres("globoticket-sql-postgres")
    .WithLifetime(ContainerLifetime.Persistent);
var mySqlDbServer = builder.AddMySql("globoticket-sql-mysql")
    .WithLifetime(ContainerLifetime.Persistent);

var eventCatalogDb = postgresDbServer.AddDatabase("globoticket-postgres-eventcatalog");
var shoppingBasketDb = mySqlDbServer.AddDatabase("globoticket-mysql-shoppingbasket");
var orderDb = mySqlDbServer.AddDatabase("globoticket-mysql-order");

var web = builder.AddProject<Projects.GloboTicket_Web>("globoticket-web")
    .WithExternalHttpEndpoints();

var storage = builder.AddAzureStorage("identity-storage")
    .RunAsEmulator(emulator => emulator.WithLifetime(ContainerLifetime.Persistent));
var keysBlob = storage.AddBlobs("keys-storage");

//var keyVault = builder.AddAzureKeyVault("identity-keyvault");

var identityService = builder.AddProject<Projects.GloboTicket_Services_IdentityServer>("globoticket-identity")
    .WithExternalHttpEndpoints()
    .WithEnvironment("GLOBOTICKET_WEB_EXTERNAL_HTTPS", web.GetEndpoint("https"))
    .WithReference(web)
    //.WithReference(keyVault)
    .WithReference(keysBlob)
    .WaitFor(keysBlob);

var eventCatalogService = builder
    .AddProject<Projects.GloboTicket_Services_EventCatalog>("globoticket-eventcatalog")
    .WithReference(eventCatalogDb)
    .WaitFor(eventCatalogDb)
    .WithReference(identityService);

var shoppingBasketService = builder
    .AddProject<Projects.GloboTicket_Services_ShoppingBasket>("globoticket-shoppingbasket")
    .WithReference(shoppingBasketDb)
    .WaitFor(shoppingBasketDb)
    .WithReference(eventCatalogService)
    .WaitFor(eventCatalogService)
    .WithReference(transport)
    .WaitFor(transport)
    .WithReference(identityService);

var orderService = builder
    .AddProject<Projects.GloboTicket_Services_Order>("globoticket-order")
    .WithReference(orderDb)
    .WaitFor(orderDb)
    .WithReference(transport)
    .WaitFor(transport)
    .WithReference(shoppingBasketService)
    .WithReference(identityService);

web.WithReference(eventCatalogService)
    .WaitFor(eventCatalogService)
    .WithReference(shoppingBasketService)
    .WaitFor(shoppingBasketService)
    .WithReference(orderService)
    .WithReference(identityService)
    .WaitFor(identityService);

var paymentService = builder
    .AddProject<Projects.GloboTicket_Services_Payment>("globoticket-payment")
    .WithReference(transport)
    .WaitFor(transport);


builder.Build().Run();
