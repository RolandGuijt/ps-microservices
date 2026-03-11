using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder();

builder.AddServiceDefaults();

var endpointConfiguration = new EndpointConfiguration("Payment");

var connectionString = builder.Configuration.GetConnectionString("transport");
var transport = new RabbitMQTransport(RoutingTopology.Conventional(QueueType.Quorum), connectionString);
var routing = endpointConfiguration.UseTransport(transport);

endpointConfiguration.UseSerialization<SystemJsonSerializer>();
endpointConfiguration.EnableInstallers();

builder.UseNServiceBus(endpointConfiguration);

await builder.Build().RunAsync();