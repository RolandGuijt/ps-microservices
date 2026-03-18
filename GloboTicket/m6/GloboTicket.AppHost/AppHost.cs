using System.Text.Json;

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
var shippingDB = postgresDbServer.AddDatabase("globoticket-postgres-shipping");
var shoppingBasketDb = mySqlDbServer.AddDatabase("globoticket-mysql-shoppingbasket");
var orderDb = mySqlDbServer.AddDatabase("globoticket-mysql-order");

var web = builder.AddProject<Projects.GloboTicket_Web>("globoticket-web");

var storage = builder.AddAzureStorage("identity-storage")
    .RunAsEmulator(emulator => emulator.WithLifetime(ContainerLifetime.Persistent));
var keysBlob = storage.AddBlobs("keys-storage");

var keyVault = builder.AddAzureKeyVault("identity-keyvault");

var identityService = builder.AddProject<Projects.GloboTicket_Services_IdentityServer>("globoticket-identity")
    .WithReference(web)
    .WithReference(keysBlob);

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

var shippingService = builder
    .AddProject<Projects.GloboTicket_Services_Shipping>("globoticket-shipping")
    .WithReference(transport)
    .WaitFor(transport)
    .WithReference(shippingDB)
    .WaitFor(shippingDB);



var ravenDB = builder.AddContainer("servicecontrol-ravendb", "particular/servicecontrol-ravendb")
    .WithContainerName("servicecontrol-ravendb")
    .WithEnvironment("RAVEN_Security_UnsecuredAccessAllowed", "PrivateNetwork")
    .WithHttpEndpoint(8080, 8080)
    .WithUrlForEndpoint("http", url => url.DisplayText = "Management Studio");

var audit = builder.AddContainer("servicecontrol-audit", "particular/servicecontrol-audit")
    .WithEnvironment("TRANSPORTTYPE", "RabbitMQ.QuorumConventionalRouting")
    .WithEnvironment("CONNECTIONSTRING", transport)
    .WithEnvironment("RAVENDB_CONNECTIONSTRING", "http://servicecontrol-ravendb:8080")
    .WithArgs("--setup-and-run")
    .WithHttpEndpoint(44444, 44444)
    .WithUrlForEndpoint("http", url => url.DisplayLocation = UrlDisplayLocation.DetailsOnly)
    .WithHttpHealthCheck("api/configuration")
    .WaitFor(transport)
    .WaitFor(ravenDB);

var serviceControl = builder.AddContainer("servicecontrol", "particular/servicecontrol")
    .WithEnvironment("TRANSPORTTYPE", "RabbitMQ.QuorumConventionalRouting")
    .WithEnvironment("CONNECTIONSTRING", transport)
    .WithEnvironment("RAVENDB_CONNECTIONSTRING", "http://servicecontrol-ravendb:8080")
    .WithEnvironment("REMOTEINSTANCES", "[{\"api_uri\":\"http://servicecontrol-audit:44444\"}]")
    .WithArgs("--setup-and-run")
    .WithHttpEndpoint(33333, 33333)
    .WithUrlForEndpoint("http", url => url.DisplayLocation = UrlDisplayLocation.DetailsOnly)
    .WithHttpHealthCheck("api/configuration")
    .WaitFor(transport)
    .WaitFor(ravenDB);

var monitoring = builder.AddContainer("servicecontrol-monitoring", "particular/servicecontrol-monitoring")
    .WithEnvironment("TRANSPORTTYPE", "RabbitMQ.QuorumConventionalRouting")
    .WithEnvironment("CONNECTIONSTRING", transport)
    .WithArgs("--setup-and-run")
    .WithHttpEndpoint(33633, 33633)
    .WithUrlForEndpoint("http", url => url.DisplayLocation = UrlDisplayLocation.DetailsOnly)
    .WithHttpHealthCheck("connection")
    .WaitFor(transport);

var servicePulse = builder.AddContainer("servicepulse", "particular/servicepulse")
    .WithEnvironment("ENABLE_REVERSE_PROXY", "false")
    .WithHttpEndpoint(9090, 9090)
    .WithUrlForEndpoint("http", url => url.DisplayText = "ServicePulse")
    .WaitFor(serviceControl)
    .WaitFor(audit)
    .WaitFor(monitoring);

builder.Build().Run();
