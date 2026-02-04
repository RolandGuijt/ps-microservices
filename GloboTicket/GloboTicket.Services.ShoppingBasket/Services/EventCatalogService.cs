using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GloboTicket.Services.ShoppingBasket.Entities;

namespace GloboTicket.Services.ShoppingBasket.Services;

public class EventCatalogService(HttpClient client) : IEventCatalogService
{
    public async Task<Event> GetEvent(Guid id)
    {
        return (await client.GetFromJsonAsync<Event>($"/api/events/{id}"))!;
    }
}
