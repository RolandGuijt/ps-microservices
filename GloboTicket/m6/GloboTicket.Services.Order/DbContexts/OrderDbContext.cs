using Microsoft.EntityFrameworkCore;

namespace GloboTicket.Services.Ordering.DbContexts;

public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public DbSet<Entities.Order> Orders { get; set; }
}
    
