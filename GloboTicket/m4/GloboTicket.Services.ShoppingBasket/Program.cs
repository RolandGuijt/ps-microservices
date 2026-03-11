using System;
using GloboTicket.Integration.Messages;
using GloboTicket.Services.ShoppingBasket.DbContexts;
using GloboTicket.Services.ShoppingBasket.Grpc;
using GloboTicket.Services.ShoppingBasket.Repositories;
using GloboTicket.Services.ShoppingBasket.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddAuthentication()
    .AddJwtBearer(opt =>
    {
        opt.Authority = "https://localhost:5000";

        opt.TokenValidationParameters.ValidateAudience = false;
        opt.TokenValidationParameters.ValidTypes = ["at+jwt"];

        opt.MapInboundClaims = false;

        opt.TokenValidationParameters.ClockSkew = TimeSpan.FromSeconds(1);
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

builder.Services.AddHttpClient<IEventCatalogService, EventCatalogService>(c =>
    c.BaseAddress = new Uri("https+http://globoticket-services-eventcatalog"));

builder.Services.AddDbContext<ShoppingBasketDbContext>(options =>
{
    options.UseMySQL(builder.Configuration.GetConnectionString("globoticket-mysql-shoppingbasket") ?? throw new InvalidOperationException());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ShoppingBasketDbContext>();
    await db.Database.MigrateAsync();
}

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapGrpcService<ShoppingBasketGrpcService>()
    .RequireAuthorization();

app.Run();
