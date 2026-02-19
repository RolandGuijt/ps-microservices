using System.Threading.Tasks;
using GloboTicket.Integration.Messages;
using GloboTicket.Services.ShoppingBasket.Repositories;
using NServiceBus;

namespace GloboTicket.Services.ShoppingBasket;

public class OrderPlacedHandler(IBasketRepository basketRepository): IHandleMessages<OrderPlaced>
{
    public async Task Handle(OrderPlaced message, IMessageHandlerContext context)
    {
        await basketRepository.RemoveBasket(message.BasketId);
    }
}