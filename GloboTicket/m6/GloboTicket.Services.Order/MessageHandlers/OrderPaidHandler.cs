using GloboTicket.Integration.Messages;
using GloboTicket.Services.Order.Repositories;

namespace GloboTicket.Services.Order;

public class OrderPaidHandler(IOrderRepository orderRepository): IHandleMessages<OrderPaid>
{
    public async Task Handle(OrderPaid message, IMessageHandlerContext context)
    {
        await orderRepository.UpdateOrderPaymentStatus(message.OrderId, true);
    }
}