var builder = DistributedApplication.CreateBuilder(args);

var postgresDbServer = builder.AddPostgres("globoticket-sql-postgres")
    .WithLifetime(ContainerLifetime.Persistent);
var mySqlDbServer = builder.AddMySql("globoticket-sql-mysql")
    .WithLifetime(ContainerLifetime.Persistent);

var eventCatalogDb = postgresDbServer.AddDatabase("globoticket-postgres-eventcatalog");
var shoppingBasketDb = mySqlDbServer.AddDatabase("globoticket-mysql-shoppingbasket");


var eventCatalogService = builder.AddProject<Projects.GloboTicket_Services_EventCatalog>("globoticket-services-eventcatalog")
    .WithReference(eventCatalogDb)
    .WaitFor(eventCatalogDb);

var shoppingBasketService = builder
    .AddProject<Projects.GloboTicket_Services_ShoppingBasket>("globoticket-services-shoppingbasket")
    .WithReference(eventCatalogService)
    .WaitFor(eventCatalogService)
    .WithReference(shoppingBasketDb)
    .WaitFor(shoppingBasketDb);

builder.AddProject<Projects.GloboTicket_Web>("globoticket-web")
    .WithReference(eventCatalogService)
    .WaitFor(eventCatalogService)
    .WithReference(shoppingBasketService)
    .WaitFor(shoppingBasketService);

builder.Build().Run();
