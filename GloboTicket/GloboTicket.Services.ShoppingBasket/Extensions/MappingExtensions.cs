using System.Collections.Generic;
using System.Linq;
using GloboTicket.Services.ShoppingBasket.Entities;
using GloboTicket.Services.ShoppingBasket.Models;

namespace GloboTicket.Services.ShoppingBasket.Extensions;

public static class MappingExtensions
{
    public static Models.Basket MapToDto(this Entities.Basket basket)
    {
        if (basket == null) return null;
        return new Models.Basket
        {
            BasketId = basket.BasketId,
            UserId = basket.UserId
        };
    }

    public static Entities.Basket MapToEntity(this BasketForCreation basketForCreation)
    {
        if (basketForCreation == null) return null;
        return new Entities.Basket
        {
            UserId = basketForCreation.UserId
        };
    }

    public static Models.BasketLine MapToDto(this Entities.BasketLine basketLine)
    {
        if (basketLine == null) return null;
        return new Models.BasketLine
        {
            BasketLineId = basketLine.BasketLineId,
            BasketId = basketLine.BasketId,
            EventId = basketLine.EventId,
            Price = basketLine.Price,
            TicketAmount = basketLine.TicketAmount,
            Event = basketLine.Event.MapToDto()
        };
    }

    public static List<Models.BasketLine> MapToDto(this IEnumerable<Entities.BasketLine> basketLines)
    {
        return basketLines?.Select(bl => bl.MapToDto()).ToList() ?? new List<Models.BasketLine>();
    }

    public static Entities.BasketLine MapToEntity(this BasketLineForCreation basketLineForCreation)
    {
        if (basketLineForCreation == null) return null;
        return new Entities.BasketLine
        {
            EventId = basketLineForCreation.EventId,
            Price = basketLineForCreation.Price,
            TicketAmount = basketLineForCreation.TicketAmount
        };
    }

    public static void MapToEntity(this BasketLineForUpdate basketLineForUpdate, Entities.BasketLine basketLine)
    {
        if (basketLineForUpdate == null || basketLine == null) return;
        basketLine.TicketAmount = basketLineForUpdate.TicketAmount;
    }

    public static Models.Event MapToDto(this Entities.Event @event)
    {
        if (@event == null) return null;
        return new Models.Event
        {
            EventId = @event.EventId,
            Name = @event.Name,
            Date = @event.Date
        };
    }
}
