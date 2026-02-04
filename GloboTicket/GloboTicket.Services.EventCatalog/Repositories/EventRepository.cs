using GloboTicket.Services.EventCatalog.DbContexts;
using GloboTicket.Services.EventCatalog.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GloboTicket.Services.EventCatalog.Repositories;

public class EventRepository(EventCatalogDbContext eventCatalogDbContext): IEventRepository
{
    public async Task<IEnumerable<Event>> GetEvents(Guid categoryId)
    {
        return await eventCatalogDbContext.Events
            .Include(x => x.Category)
            .Where(x => (x.CategoryId == categoryId || categoryId == Guid.Empty)).ToListAsync();
    }

    public async Task<Event> GetEventById(Guid eventId)
    {
        return await eventCatalogDbContext.Events.Include(x => x.Category).Where(x => x.EventId == eventId).FirstOrDefaultAsync();
    }
}
