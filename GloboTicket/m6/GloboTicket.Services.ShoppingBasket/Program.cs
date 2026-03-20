using System;
using System.Threading.Tasks;
using Duende.AccessTokenManagement;
using GloboTicket.Integration.Messages;
using GloboTicket.Services.ShoppingBasket;
using GloboTicket.Services.ShoppingBasket.DbContexts;
using GloboTicket.Services.ShoppingBasket.Grpc;
using GloboTicket.Services.ShoppingBasket.Repositories;
using GloboTicket.Services.ShoppingBasket.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

builder.AddServiceDefaults();

builder.Services.AddAuthentication()
    .AddJwtBearer(opt =>
    {
        opt.Authority = builder.Configuration["GLOBOTICKET_IDENTITY_HTTPS"];

        opt.TokenValidationParameters.ValidateAudience = false;
        opt.TokenValidationParameters.ValidTypes = ["at+jwt"];

        opt.MapInboundClaims = false;
    });

builder.Services.AddAuthorization()
    .AddAuthorizationBuilder()
    .AddPolicy("shopping-basket-scope", p =>
    {
        p.RequireAuthenticatedUser();
        p.RequireClaim("scope", "shopping-basket");
    });

builder.Services.AddClientCredentialsTokenManagement()
    .AddClient(ClientCredentialsClientName.Parse("event-catalog-client"), client =>
    {
        client.TokenEndpoint = new Uri(builder.Configuration["GLOBOTICKET_IDENTITY_HTTPS"] + "/connect/token");

        client.ClientId = ClientId.Parse("ShoppingBasket");
        client.ClientSecret = ClientSecret.Parse("wexite43");

        client.Scope = Scope.Parse("event-catalog");
    });

var endpointConfiguration = new EndpointConfiguration("ShoppingCart");
endpointConfiguration.UseSerialization<SystemJsonSerializer>();

endpointConfiguration.EnableInstallers();

var connectionString = builder.Configuration.GetConnectionString("transport");
var transport = new RabbitMQTransport(RoutingTopology.Conventional(QueueType.Quorum), connectionString);
var routing = endpointConfiguration.UseTransport(transport);
routing.RouteToEndpoint(typeof(PlaceOrder),"Order");

builder.UseNServiceBus(endpointConfiguration);

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.AddScoped<IBasketLinesRepository, BasketLinesRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<TokenForwardingHandler>();


builder.Services.AddHttpClient<IEventCatalogService, EventCatalogService>(c =>
        c.BaseAddress = new Uri("https+http://globoticket-eventcatalog"))
    //.AddHttpMessageHandler<TokenForwardingHandler>();
    .AddClientCredentialsTokenHandler(ClientCredentialsClientName.Parse("event-catalog-client"));

builder.Services.AddDbContext<ShoppingBasketDbContext>(options =>
{
    options.UseMySQL(builder.Configuration.GetConnectionString("globoticket-mysql-shoppingbasket") ?? throw new InvalidOperationException());
});

var app = builder.Build();

app.Lifetime.ApplicationStarted.Register(() =>
{
    _ = Task.Run(async () =>
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ShoppingBasketDbContext>();
        await db.Database.MigrateAsync();
    });
});

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseAuthorization();

app.MapGrpcService<ShoppingBasketGrpcService>()
    .RequireAuthorization("shopping-basket-scope");

app.Run();
