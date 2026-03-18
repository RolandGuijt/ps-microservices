using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Npgsql;
using NpgsqlTypes;

var builder = Host.CreateApplicationBuilder();

builder.AddServiceDefaults();

var endpointConfiguration = new EndpointConfiguration("Shipping");

var connectionString = builder.Configuration.GetConnectionString("transport");
var transport = new RabbitMQTransport(RoutingTopology.Conventional(QueueType.Quorum), connectionString);
var routing = endpointConfiguration.UseTransport(transport);

var persistenceConnection = 
    builder.Configuration.GetConnectionString("globoticket-postgres-shipping");
var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
persistence.ConnectionBuilder(
    connectionBuilder: () => new NpgsqlConnection(persistenceConnection));

var dialect = persistence.SqlDialect<SqlDialect.PostgreSql>();
dialect.JsonBParameterModifier(
    modifier: parameter =>
    {
        var npgsqlParameter = (NpgsqlParameter)parameter;
        npgsqlParameter.NpgsqlDbType = NpgsqlDbType.Jsonb;
    });

endpointConfiguration.UseSerialization<SystemJsonSerializer>();
endpointConfiguration.EnableInstallers();

builder.UseNServiceBus(endpointConfiguration);

await builder.Build().RunAsync();