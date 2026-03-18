using GloboTicket.Integration.Messages;
using Microsoft.Extensions.Logging;

namespace GloboTicket.Services.Shipping;

class ShipOrderHandler(ILogger<ShipOrderHandler> log) : 
    IHandleMessages<ShipOrder>
{
    public Task Handle(ShipOrder message, IMessageHandlerContext context)
    {
        log.LogInformation("Order [{OrderId}] - Successfully shipped.", 
            message.OrderId);
        return Task.CompletedTask;
    }
}
