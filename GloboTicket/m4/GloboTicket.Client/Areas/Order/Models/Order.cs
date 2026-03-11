using System;

namespace GloboTicket.Web.Areas.Order.Models;

public record Order(Guid Id, Guid UserId, int OrderTotal, DateTime OrderPlaced, bool OrderPaid)
{
}