using System;
using System.Linq;
using System.Threading.Tasks;
using GloboTicket.Services.ShoppingBasket.Entities;
using GloboTicket.Services.ShoppingBasket.Repositories;
using GloboTicket.Services.ShoppingBasket.Services;
using Grpc.Core;

namespace GloboTicket.Services.ShoppingBasket.Grpc;

public class ShoppingBasketGrpcService(
    IBasketRepository basketRepository,
    IBasketLinesRepository basketLinesRepository,
    IEventRepository eventRepository,
    IEventCatalogService eventCatalogService) : ShoppingBasketService.ShoppingBasketServiceBase
{
    public override async Task<BasketReply> GetBasket(GetBasketRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.BasketId, out var basketId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid basket ID format"));
        }

        var basket = await basketRepository.GetBasketById(basketId);
        if (basket == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Basket not found"));
        }

        var numberOfItems = basket.BasketLines.Sum(bl => bl.TicketAmount);

        return new BasketReply
        {
            BasketId = basket.BasketId.ToString(),
            UserId = basket.UserId.ToString(),
            NumberOfItems = numberOfItems
        };
    }
    
    public override async Task<BasketReply> CreateBasket(CreateBasketRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user ID format"));
        }

        var basket = new Basket
        {
            BasketId = Guid.NewGuid(),
            UserId = userId
        };

        basketRepository.AddBasket(basket);
        await basketRepository.SaveChanges();

        return new BasketReply
        {
            BasketId = basket.BasketId.ToString(),
            UserId = basket.UserId.ToString(),
            NumberOfItems = 0
        };
    }

    public override async Task<BasketLinesReply> GetBasketLines(GetBasketLinesRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.BasketId, out var basketId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid basket ID format"));
        }

        var basketLines = await basketLinesRepository.GetBasketLines(basketId);
        var reply = new BasketLinesReply();

        foreach (var line in basketLines)
        {
            reply.BasketLines.Add(new BasketLineReply
            {
                BasketLineId = line.BasketLineId.ToString(),
                BasketId = line.BasketId.ToString(),
                EventId = line.EventId.ToString(),
                Price = line.Price,
                TicketAmount = line.TicketAmount
            });
        }

        return reply;
    }

    public override async Task<BasketLineReply> AddOrUpdateBasketLine(AddOrUpdateBasketLineRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.BasketId, out var basketId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid basket ID format"));
        }

        if (!Guid.TryParse(request.EventId, out var eventId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid event ID format"));
        }
        
        if (!await eventRepository.EventExists(eventId))
        {
            var eventFromCatalog = await eventCatalogService.GetEvent(eventId);
            eventRepository.AddEvent(eventFromCatalog);
            await eventRepository.SaveChanges();
        }

        Guid? basketLineId = null;
        if (!string.IsNullOrEmpty(request.BasketLineId) && Guid.TryParse(request.BasketLineId, out var parsedBasketLineId))
        {
            basketLineId = parsedBasketLineId;
        }

        var basketLine = new BasketLine
        {
            BasketLineId = basketLineId ?? Guid.NewGuid(),
            BasketId = basketId,
            EventId = eventId,
            Price = request.Price,
            TicketAmount = request.TicketAmount
        };

        var result = await basketLinesRepository.AddOrUpdateBasketLine(basketId, basketLine);
        await basketLinesRepository.SaveChanges();

        return new BasketLineReply
        {
            BasketLineId = result.BasketLineId.ToString(),
            BasketId = result.BasketId.ToString(),
            EventId = result.EventId.ToString(),
            Price = result.Price,
            TicketAmount = result.TicketAmount
        };
    }

    public override async Task<RemoveBasketLineReply> RemoveBasketLine(RemoveBasketLineRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.BasketLineId, out var basketLineId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid basket line ID format"));
        }

        var basketLine = await basketLinesRepository.GetBasketLineById(basketLineId);
        if (basketLine == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Basket line not found"));
        }

        basketLinesRepository.RemoveBasketLine(basketLine);
        var success = await basketLinesRepository.SaveChanges();

        return new RemoveBasketLineReply
        {
            Success = success
        };
    }
}
