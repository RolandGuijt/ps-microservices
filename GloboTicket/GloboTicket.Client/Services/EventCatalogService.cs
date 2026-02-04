using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GloboTicket.Web.Models.Api;

namespace GloboTicket.Web.Services;

public class EventCatalogService(HttpClient client) : IEventCatalogService
{
    public async Task<IEnumerable<Event>> GetAll()
    {
        return await client.GetFromJsonAsync<List<Event>>("/api/events") ?? [];
    }

    public async Task<IEnumerable<Event>> GetByCategoryId(Guid categoryid)
    {
        return await client.GetFromJsonAsync<List<Event>>($"/api/events/?categoryId={categoryid}") ?? [];
    }

    public async Task<Event> GetEvent(Guid id)
    {
        return (await client.GetFromJsonAsync<Event>($"/api/events/{id}"))!;
    }

    public async Task<IEnumerable<Category>> GetCategories()
    {
        return await client.GetFromJsonAsync<List<Category>>("/api/categories") ?? [];
    }
}
