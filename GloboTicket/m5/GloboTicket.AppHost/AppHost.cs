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

var identityService = builder.AddProject<Projects.GloboTicket_Services_IdentityServer>("globoticket-services-identity");

var eventCatalogService = builder
    .AddProject<Projects.GloboTicket_Services_EventCatalog>("globoticket-services-eventcatalog")
    .WithReference(eventCatalogDb)
    .WaitFor(eventCatalogDb)
    .WithReference(identityService);

var shoppingBasketService = builder
    .AddProject<Projects.GloboTicket_Services_ShoppingBasket>("globoticket-services-shoppingbasket")
    .WithReference(shoppingBasketDb)
    .WaitFor(shoppingBasketDb)
    .WithReference(eventCatalogService)
    .WaitFor(eventCatalogService)
    .WithReference(transport)
    .WaitFor(transport)
    .WithReference(identityService);

var orderService = builder
    .AddProject<Projects.GloboTicket_Services_Order>("globoticket-services-order")
    .WithReference(orderDb)
    .WaitFor(orderDb)
    .WithReference(transport)
    .WaitFor(transport)
    .WithReference(shoppingBasketService)
    .WithReference(identityService);

var paymentService = builder
    .AddProject<Projects.GloboTicket_Services_Payment>("globoticket-services-payment")
    .WithReference(transport)
    .WaitFor(transport);

var shippingService = builder
    .AddProject<Projects.GloboTicket_Services_Shipping>("globoticket-services-shipping")
    .WithReference(transport)
    .WaitFor(transport)
    .WithReference(shippingDB)
    .WaitFor(shippingDB);

builder.AddProject<Projects.GloboTicket_Web>("globoticket-web")
    .WithReference(eventCatalogService)
    .WaitFor(eventCatalogService)
    .WithReference(shoppingBasketService)
    .WaitFor(shoppingBasketService)
    .WithReference(orderService)
    .WithReference(identityService)
    .WaitFor(identityService);

var ravenDB = builder.AddContainer("ServiceControl-RavenDB", "particular/servicecontrol-ravendb")
    .WithHttpEndpoint(8080, 8080)
    .WithUrlForEndpoint("http", url => url.DisplayText = "Management Studio");

var audit = builder.AddContainer("ServiceControl-Audit", "particular/servicecontrol-audit")
    .WithEnvironment("TRANSPORTTYPE", "RabbitMQ.QuorumConventionalRouting")
    .WithEnvironment("CONNECTIONSTRING", transport)
    .WithEnvironment("RAVENDB_CONNECTIONSTRING", ravenDB.GetEndpoint("http"))
    .WithArgs("--setup-and-run")
    .WithHttpEndpoint(44444, 44444)
    .WithUrlForEndpoint("http", url => url.DisplayLocation = UrlDisplayLocation.DetailsOnly)
    .WithHttpHealthCheck("api/configuration")
    .WaitFor(transport)
    .WaitFor(ravenDB);

var monitoring = builder.AddContainer("ServiceControl-Monitoring", "particular/servicecontrol-monitoring")
    .WithEnvironment("TRANSPORTTYPE", "RabbitMQ.QuorumConventionalRouting")
    .WithEnvironment("CONNECTIONSTRING", transport)
    .WithArgs("--setup-and-run")
    .WithHttpEndpoint(33633, 33633)
    .WithUrlForEndpoint("http", url => url.DisplayLocation = UrlDisplayLocation.DetailsOnly)
    .WithHttpHealthCheck("connection")
    .WaitFor(transport);

var serviceControl = builder.AddContainer("ServiceControl", "particular/servicecontrol:latest")
    .WithEnvironment("TRANSPORTTYPE", "RabbitMQ.QuorumConventionalRouting")
    .WithEnvironment("CONNECTIONSTRING", transport)
    .WithEnvironment("RAVENDB_CONNECTIONSTRING", ravenDB.GetEndpoint("http"))
    .WithEnvironment("REMOTEINSTANCES", $"[{{\"api_uri\":\"{audit.GetEndpoint("http")}\"}}]")
    .WithEnvironment("ENABLEINTEGRATEDSERVICEPULSE", "true")
    .WithArgs("--setup-and-run")
    .WithHttpEndpoint(33333, 33333)
    .WithUrlForEndpoint("http", url => url.DisplayText = "ServicePulse")
    .WithEnvironment("ENABLE_REVERSE_PROXY", "false")
    .WithHttpHealthCheck("api/configuration")
    .WaitFor(monitoring)
    .WaitFor(transport)
    .WaitFor(ravenDB);



builder.Build().Run();
