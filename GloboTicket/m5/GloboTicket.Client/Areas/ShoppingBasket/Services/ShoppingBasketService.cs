using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GloboTicket.Services.ShoppingBasket.Grpc;
using GloboTicket.Web.Models;
using GloboTicket.Web.Models.Api;
using GrpcShoppingBasketService = GloboTicket.Services.ShoppingBasket.Grpc.ShoppingBasketService;

namespace GloboTicket.Web.Services;

public class ShoppingBasketService(
    GrpcShoppingBasketService.ShoppingBasketServiceClient grpcClient,
    IEventCatalogService eventCatalogService,
    Settings settings): IShoppingBasketService
{

    public async Task<BasketLine> AddToBasket(Guid basketId, BasketLineForCreation basketLine)
    {
        if (basketId == Guid.Empty)
        {
            var createRequest = new CreateBasketRequest
            {
                UserId = settings.UserId.ToString()
            };
            var basketResponse = await grpcClient.CreateBasketAsync(createRequest);
            basketId = Guid.Parse(basketResponse.BasketId);
        }

        var addRequest = new AddOrUpdateBasketLineRequest
        {
            BasketId = basketId.ToString(),
            EventId = basketLine.EventId.ToString(),
            Price = basketLine.Price,
            TicketAmount = basketLine.TicketAmount
        };

        var response = await grpcClient.AddOrUpdateBasketLineAsync(addRequest);

        // Get event details for the basket line
        var eventDetails = await eventCatalogService.GetEvent(basketLine.EventId);

        return new BasketLine
        {
            BasketLineId = Guid.Parse(response.BasketLineId),
            BasketId = Guid.Parse(response.BasketId),
            EventId = Guid.Parse(response.EventId),
            Price = response.Price,
            TicketAmount = response.TicketAmount,
            Event = new Event
            {
                EventId = eventDetails.EventId,
                Name = eventDetails.Name,
                Date = eventDetails.Date
            }
        };
    }

    public async Task<Basket> GetBasket(Guid basketId)
    {
        if (basketId == Guid.Empty)
            return null;

        var request = new GetBasketRequest
        {
            BasketId = basketId.ToString()
        };

        var response = await grpcClient.GetBasketAsync(request);

        return new Basket
        {
            BasketId = Guid.Parse(response.BasketId),
            UserId = Guid.Parse(response.UserId),
            NumberOfItems = response.NumberOfItems
        };
    }

    public async Task<IEnumerable<BasketLine>> GetLinesForBasket(Guid basketId)
    {
        if (basketId == Guid.Empty)
            return [];

        var request = new GetBasketLinesRequest
        {
            BasketId = basketId.ToString()
        };

        var response = await grpcClient.GetBasketLinesAsync(request);

        var basketLines = new List<BasketLine>();

        foreach (var line in response.BasketLines)
        {
            var eventId = Guid.Parse(line.EventId);
            var eventDetails = await eventCatalogService.GetEvent(eventId);

            basketLines.Add(new BasketLine
            {
                BasketLineId = Guid.Parse(line.BasketLineId),
                BasketId = Guid.Parse(line.BasketId),
                EventId = eventId,
                Price = line.Price,
                TicketAmount = line.TicketAmount,
                Event = new Event
                {
                    EventId = eventDetails.EventId,
                    Name = eventDetails.Name,
                    Date = eventDetails.Date
                }
            });
        }

        return basketLines;
    }

    public async Task UpdateLine(Guid basketId, BasketLineForUpdate basketLineForUpdate)
    {
        await grpcClient.UpdateBasketLineAsync(new UpdateBasketLineRequest
        {
            BasketId = basketId.ToString(),
            LineId = basketLineForUpdate.LineId.ToString(),
            TicketAmount = basketLineForUpdate.TicketAmount
        });
    }

    public async Task RemoveLine(Guid basketId, Guid lineId)
    {
        var request = new RemoveBasketLineRequest
        {
            BasketLineId = lineId.ToString()
        };

        await grpcClient.RemoveBasketLineAsync(request);
    }

    public async Task Checkout(Guid basketId, int totalPrice, string cardnumber)
    {
        await grpcClient.CheckoutBasketAsync(new CheckoutBasketRequest
        {
            UserId = settings.UserId.ToString(),
            BasketId = basketId.ToString(),
            BasketTotal = totalPrice,
            CardNumber = cardnumber
        });
    }
}
