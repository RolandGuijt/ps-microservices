using System;
using GloboTicket.Services.ShoppingBasket.DbContexts;
using GloboTicket.Services.ShoppingBasket.Repositories;
using GloboTicket.Services.ShoppingBasket.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddControllers();

builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.AddScoped<IBasketLinesRepository, BasketLinesRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();

builder.Services.AddHttpClient<IEventCatalogService, EventCatalogService>(c =>
    c.BaseAddress = new Uri("https+http://globoticket-services-eventcatalog"));

builder.Services.AddDbContext<ShoppingBasketDbContext>(options =>
{
    options.UseMySQL(builder.Configuration.GetConnectionString("globoticket-mysql-shoppingbasket") ?? throw new InvalidOperationException());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

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

app.MapGrpcService<GloboTicket.Services.ShoppingBasket.Grpc.ShoppingBasketGrpcService>();

app.MapOpenApi();
app.MapScalarApiReference();
app.UseAuthorization();
app.MapControllers();

app.Run();
