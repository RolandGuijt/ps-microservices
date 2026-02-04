using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GloboTicket.Web.Models;
using GloboTicket.Web.Models.Api;

namespace GloboTicket.Web.Services
{
    public class ShoppingBasketService: IShoppingBasketService
    {
        private readonly HttpClient client;
        private readonly Settings settings;

        public ShoppingBasketService(HttpClient client, Settings settings)
        {
            this.client = client;
            this.settings = settings;
        }

        public async Task<BasketLine> AddToBasket(Guid basketId, BasketLineForCreation basketLine)
        {
            if (basketId == Guid.Empty)
            {
                var basketResponse = await client.PostAsJsonAsync("/api/baskets", new BasketForCreation { UserId = settings.UserId });
                var basket = await basketResponse.Content.ReadFromJsonAsync<Basket>();
                basketId = basket.BasketId;
            }

            var response = await client.PostAsJsonAsync($"api/baskets/{basketId}/basketlines", basketLine);
            return await response.Content.ReadFromJsonAsync<BasketLine>();
        }

        public async Task<Basket> GetBasket(Guid basketId)
        {
            if (basketId == Guid.Empty)
                return null;
            return await client.GetFromJsonAsync<Basket>($"/api/baskets/{basketId}");
        }

        public async Task<IEnumerable<BasketLine>> GetLinesForBasket(Guid basketId)
        {
            if (basketId == Guid.Empty)
                return new BasketLine[0];
            return await client.GetFromJsonAsync<BasketLine[]>($"/api/baskets/{basketId}/basketLines");
        }

        public async Task UpdateLine(Guid basketId, BasketLineForUpdate basketLineForUpdate)
        {
            await client.PutAsJsonAsync($"/api/baskets/{basketId}/basketLines/{basketLineForUpdate.LineId}", basketLineForUpdate);
        }

        public async Task RemoveLine(Guid basketId, Guid lineId)
        {
            await client.DeleteAsync($"/api/baskets/{basketId}/basketLines/{lineId}");
        }
    }
}
