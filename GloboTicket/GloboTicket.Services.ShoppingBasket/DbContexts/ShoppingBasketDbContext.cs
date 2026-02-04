using GloboTicket.Services.ShoppingBasket.Entities;
using Microsoft.EntityFrameworkCore;

namespace GloboTicket.Services.ShoppingBasket.DbContexts;

public class ShoppingBasketDbContext(DbContextOptions<ShoppingBasketDbContext> options) : DbContext(options)
{
    public DbSet<Basket> Baskets { get; set; }
    public DbSet<BasketLine> BasketLines { get; set; }
    public DbSet<Event> Events { get; set; }
}
