using GloboTicket.Integration.Messages;
using Microsoft.Extensions.Logging;

namespace GloboTicket.Services.Shipping;

class ShippingPolicy(ILogger<ShippingPolicy> log) : Saga<ShippingPolicyData>,
    IAmStartedByMessages<OrderPlaced>,
    IAmStartedByMessages<OrderPaid>
{
    protected override void ConfigureHowToFindSaga(
        SagaPropertyMapper<ShippingPolicyData> mapper)
    {
        mapper.MapSaga(sagaData => sagaData.OrderId)
            .ToMessage<OrderPlaced>(message => message.OrderId)
            .ToMessage<OrderPaid>(message => message.OrderId);
    }

    public Task Handle(OrderPlaced message, IMessageHandlerContext context)
    {
        log.LogInformation("OrderPlaced message received.");
        Data.IsOrderPlaced = true;

        return ProcessOrder(context);
    }

    public Task Handle(OrderPaid message, IMessageHandlerContext context)
    {
        log.LogInformation("OrderBilled message received.");
        Data.IsOrderBilled = true;

        return ProcessOrder(context);
    }

    private async Task ProcessOrder(IMessageHandlerContext context)
    {
        if (Data.IsOrderPlaced && Data.IsOrderBilled)
        {
            await context.SendLocal(new ShipOrder() { OrderId = Data.OrderId });
            MarkAsComplete();
        }
    }
}
