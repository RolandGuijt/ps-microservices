using GloboTicket.Services.Ordering.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace GloboTicket.Services.Order.Repositories;

public class OrderRepository(OrderDbContext dbContext): IOrderRepository
{
    public async Task<List<Ordering.Entities.Order>> GetOrdersForUser(Guid userId)
    {
        return await dbContext.Orders.Where(o => o.UserId == userId).OrderBy(o => o.OrderPlaced).ToListAsync();
    }

    public async Task AddOrder(Ordering.Entities.Order order)
    {
        await dbContext.Orders.AddAsync(order);
        await dbContext.SaveChangesAsync();
    }

    public async Task<Ordering.Entities.Order> GetOrderById(Guid orderId)
    {
        return await dbContext.Orders.Where(o => o.Id == orderId).FirstAsync();
    }

    public async Task UpdateOrderPaymentStatus(Guid orderId, bool paid)
    {
            var order = await dbContext.Orders.Where(o => o.Id == orderId).FirstAsync();
            order.OrderPaid = paid;
            await dbContext.SaveChangesAsync();
    }
}