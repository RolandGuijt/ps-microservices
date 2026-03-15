using GloboTicket.Services.Order.Models;

namespace GloboTicket.Services.Order.Extensions;

public static class MappingExtensions
{
    public static List<OrderDto> MapToDto(this IEnumerable<Ordering.Entities.Order> orders)
    {
        var result = orders?.Select(c => c.MapToDto()).ToList() ?? [];
        return result;
    }

    public static OrderDto MapToDto(this Ordering.Entities.Order order)
    {
        var dto = new OrderDto(order.Id, order.UserId, order.OrderTotal, order.OrderPlaced, order.OrderPaid);
        return dto;
    }
}