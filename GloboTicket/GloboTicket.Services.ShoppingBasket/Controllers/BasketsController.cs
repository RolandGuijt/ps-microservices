using System;
using System.Linq;
using System.Threading.Tasks;
using GloboTicket.Services.ShoppingBasket.Extensions;
using GloboTicket.Services.ShoppingBasket.Models;
using GloboTicket.Services.ShoppingBasket.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GloboTicket.Services.ShoppingBasket.Controllers;

[Route("api/baskets")]
[ApiController]
public class BasketsController(IBasketRepository basketRepository) : ControllerBase
{
    [HttpGet("{basketId}", Name = "GetBasket")]
    public async Task<ActionResult<Basket>> Get(Guid basketId)
    {
        var basket = await basketRepository.GetBasketById(basketId);
        if (basket == null)
        {
            return NotFound();
        }

        var result = basket.MapToDto();
        result.NumberOfItems = basket.BasketLines.Sum(bl => bl.TicketAmount);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Basket>> Post(BasketForCreation basketForCreation)
    {
        var basketEntity = basketForCreation.MapToEntity();

        basketRepository.AddBasket(basketEntity);
        await basketRepository.SaveChanges();

        var basketToReturn = basketEntity.MapToDto();

        return CreatedAtRoute(
            "GetBasket",
            new { basketId = basketEntity.BasketId },
            basketToReturn);
    }
}
