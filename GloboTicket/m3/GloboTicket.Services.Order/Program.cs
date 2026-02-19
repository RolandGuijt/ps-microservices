using GloboTicket.Services.Order.Extensions;
using GloboTicket.Services.Order.Repositories;
using GloboTicket.Services.Ordering.DbContexts;
using Microsoft.EntityFrameworkCore;
using EndpointConfiguration = NServiceBus.EndpointConfiguration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.AddServiceDefaults();

var endpointConfiguration = new EndpointConfiguration("Order");
var connectionString = builder.Configuration.GetConnectionString("transport");
var transport = new RabbitMQTransport(RoutingTopology.Conventional(QueueType.Quorum), connectionString);
endpointConfiguration.UseTransport(transport);

endpointConfiguration.UseSerialization<SystemJsonSerializer>();

endpointConfiguration.EnableInstallers();

builder.UseNServiceBus(endpointConfiguration);

builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddDbContext<OrderDbContext>(options =>
{
    var mySql = builder.Configuration.GetConnectionString("globoticket-mysql-order");
    options.UseMySQL( mySql ?? throw new InvalidOperationException());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    await db.Database.MigrateAsync();
}

app.MapGet("/orders/{userId}", async (Guid userId, IOrderRepository orderRepository) =>
{
    var orders = await orderRepository.GetOrdersForUser(userId);
    return orders.MapToDto();
});

app.MapOpenApi();
app.UseHttpsRedirection();

app.Run();