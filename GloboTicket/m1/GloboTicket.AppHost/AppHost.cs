var builder = DistributedApplication.CreateBuilder(args);

var postgresDbServer = 
    builder.AddPostgres("globoticket-sql-postgres")
        .WithLifetime(ContainerLifetime.Persistent);

var eventCatalogDb = 
    postgresDbServer.AddDatabase("globoticket-postgres-eventcatalog");

var eventCatalogService = 
    builder.AddProject<Projects.GloboTicket_Services_EventCatalog>("globoticket-services-eventcatalog")
        .WithReference(eventCatalogDb)
        .WaitFor(eventCatalogDb);

builder.AddProject<Projects.GloboTicket_Web>("globoticket-web")
    .WithReference(eventCatalogService)
    .WaitFor(eventCatalogService);

builder.Build().Run();
