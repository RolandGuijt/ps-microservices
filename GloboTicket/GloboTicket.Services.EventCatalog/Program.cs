using System;
using GloboTicket.Services.EventCatalog;
using GloboTicket.Services.EventCatalog.DbContexts;
using GloboTicket.Services.EventCatalog.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
var connection = builder.Configuration.GetConnectionString("globoticket-postgres-eventcatalog") ?? throw new InvalidOperationException();
builder.Services.AddDbContext<EventCatalogDbContext>(options =>
    options.UseNpgsql(connection)
        .UseSeeding((context, _) =>
        {
            SampleData.GetSeedData(context);
            context.SaveChanges();
        })
        .UseAsyncSeeding(async (context, _, cancellationToken) =>
        {
            SampleData.GetSeedData(context);
            await context.SaveChangesAsync(cancellationToken);
        }));


builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EventCatalogDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.MapOpenApi();
app.MapScalarApiReference();
app.UseAuthorization();
app.MapControllers();

app.Run();
