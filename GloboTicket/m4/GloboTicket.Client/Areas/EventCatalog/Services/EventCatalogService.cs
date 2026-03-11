using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GloboTicket.Web.Models.Api;

namespace GloboTicket.Web.Services;

public class EventCatalogService(IHttpClientFactory clientFactory) : IEventCatalogService
{
    private readonly HttpClient _client = clientFactory.CreateClient("event-catalog-client");

    public async Task<IEnumerable<Event>> GetAll()
    {
        return await _client.GetFromJsonAsync<List<Event>>("/api/events") ?? [];
    }

    public async Task<IEnumerable<Event>> GetByCategoryId(Guid categoryid)
    {
        return await _client.GetFromJsonAsync<List<Event>>($"/api/events/?categoryId={categoryid}") ?? [];
    }

    public async Task<Event> GetEvent(Guid id)
    {
        return (await _client.GetFromJsonAsync<Event>($"/api/events/{id}"))!;
    }

    public async Task<IEnumerable<Category>> GetCategories()
    {
        return await _client.GetFromJsonAsync<List<Category>>("/api/categories") ?? [];
    }
}
