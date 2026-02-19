using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GloboTicket.Web.Areas.Order.Models;

namespace GloboTicket.Web.Services
{
    public class OrderService(HttpClient client): IOrderService
    {
        public async Task<List<Order>> GetOrdersForUser(Guid userId)
        {
            return await client.GetFromJsonAsync<List<Order>>($"/orders/{userId}");
        }
    }
}
