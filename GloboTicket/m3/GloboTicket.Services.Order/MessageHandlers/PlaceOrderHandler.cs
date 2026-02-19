using GloboTicket.Integration.Messages;
using GloboTicket.Services.Order.Repositories;

namespace GloboTicket.Services.Order;

public class PlaceOrderHandler(IOrderRepository orderRepository): IHandleMessages<PlaceOrder>
{
    public async Task Handle(PlaceOrder message, IMessageHandlerContext context)
    {
        var orderId = Guid.NewGuid();
        
        await orderRepository.AddOrder(new Ordering.Entities.Order
        {
            Id = orderId,
            OrderTotal = message.BasketTotal,
            OrderPaid = false,
            UserId = message.UserId,
            OrderPlaced = DateTime.Now
        });

        await context.Publish(new OrderPlaced(orderId, message.UserId,  message.BasketId, message.BasketTotal));
    }
}