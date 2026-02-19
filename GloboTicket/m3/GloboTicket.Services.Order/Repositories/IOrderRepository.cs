namespace GloboTicket.Services.Order.Repositories;

public interface IOrderRepository
{
    Task<List<Ordering.Entities.Order>> GetOrdersForUser(Guid userId);
    Task AddOrder(Ordering.Entities.Order order);
    Task<Ordering.Entities.Order> GetOrderById(Guid orderId);
    Task UpdateOrderPaymentStatus(Guid orderId, bool paid);
}