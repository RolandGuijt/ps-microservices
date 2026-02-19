namespace GloboTicket.Services.Order.Models;

public record OrderDto(Guid Id, Guid UserId, int OrderTotal, DateTime OrderPlaced, bool OrderPaid);