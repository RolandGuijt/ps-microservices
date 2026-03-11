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

var eventCatalogService = builder.AddProject<Projects.GloboTicket_Services_EventCatalog>("globoticket-services-eventcatalog")
    .WithReference(eventCatalogDb)
    .WaitFor(eventCatalogDb);

var shoppingBasketService = builder
    .AddProject<Projects.GloboTicket_Services_ShoppingBasket>("globoticket-services-shoppingbasket")
    .WithReference(shoppingBasketDb)
    .WaitFor(shoppingBasketDb)
    .WithReference(eventCatalogService)
    .WaitFor(eventCatalogService)
    .WithReference(transport)
    .WaitFor(transport);

var orderService = builder
    .AddProject<Projects.GloboTicket_Services_Order>("globoticket-services-order")
    .WithReference(orderDb)
    .WaitFor(orderDb)
    .WithReference(transport)
    .WaitFor(transport);

var paymentService = builder
    .AddProject<Projects.GloboTicket_Services_Payment>("globoticket-services-payment")
    .WithReference(transport)
    .WaitFor(transport);

builder.AddProject<Projects.GloboTicket_Web>("globoticket-web")
    .WithReference(eventCatalogService)
    .WaitFor(eventCatalogService)
    .WithReference(shoppingBasketService)
    .WaitFor(shoppingBasketService)
    .WithReference(orderService);

builder.AddProject<Projects.GloboTicket_Services_IdentityServer>("globoticket-services-identity")
    .WithReference(eventCatalogService)
    .WithReference(shoppingBasketService)
    .WithReference(orderService);

builder.Build().Run();
