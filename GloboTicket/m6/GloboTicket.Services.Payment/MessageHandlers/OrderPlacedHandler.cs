using GloboTicket.Integration.Messages;

namespace GloboTicket.Services.Payment;

public class OrderPlacedHandler: IHandleMessages<OrderPlaced>
{
    public async Task Handle(OrderPlaced message, IMessageHandlerContext context)
    {
        //process payment
        await context.Publish(new OrderPaid(message.OrderId));
    }
}