using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GloboTicket.Services.ShoppingBasket.Extensions;
using GloboTicket.Services.ShoppingBasket.Models;
using GloboTicket.Services.ShoppingBasket.Repositories;
using GloboTicket.Services.ShoppingBasket.Services;
using Microsoft.AspNetCore.Mvc;

namespace GloboTicket.Services.ShoppingBasket.Controllers;

[Route("api/baskets/{basketId}/basketlines")]
[ApiController]
public class BasketLinesController(IBasketRepository basketRepository,
    IBasketLinesRepository basketLinesRepository, IEventRepository eventRepository,
    IEventCatalogService eventCatalogService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BasketLine>>> Get(Guid basketId)
    {
        if (!await basketRepository.BasketExists(basketId))
        {
            return NotFound();
        }

        var basketLines = await basketLinesRepository.GetBasketLines(basketId);
        return Ok(basketLines.MapToDto());
    }

    [HttpGet("{basketLineId}", Name = "GetBasketLine")]
    public async Task<ActionResult<BasketLine>> Get(Guid basketId,
        Guid basketLineId)
    {
        if (!await basketRepository.BasketExists(basketId))
        {
            return NotFound();
        }

        var basketLine = await basketLinesRepository.GetBasketLineById(basketLineId);
        if (basketLine == null)
        {
            return NotFound();
        }

        return Ok(basketLine.MapToDto());
    }

    [HttpPost]
    public async Task<ActionResult<BasketLine>> Post(Guid basketId,
        [FromBody] BasketLineForCreation basketLineForCreation)
    {
        if (!await basketRepository.BasketExists(basketId))
        {
            return NotFound();
        }

        if (!await eventRepository.EventExists(basketLineForCreation.EventId))
        {
            var eventFromCatalog = await eventCatalogService.GetEvent(basketLineForCreation.EventId);
            eventRepository.AddEvent(eventFromCatalog);
            await eventRepository.SaveChanges();
        }

        var basketLineEntity = basketLineForCreation.MapToEntity();

        var processedBasketLine = await basketLinesRepository.AddOrUpdateBasketLine(basketId, basketLineEntity);
        await basketLinesRepository.SaveChanges();

        var basketLineToReturn = processedBasketLine.MapToDto();

        return CreatedAtRoute(
            "GetBasketLine",
            new { basketId = basketLineEntity.BasketId, basketLineId = basketLineEntity.BasketLineId },
            basketLineToReturn);
    }

    [HttpPut("{basketLineId}")]
    public async Task<ActionResult<BasketLine>> Put(Guid basketId,
        Guid basketLineId,
        [FromBody] BasketLineForUpdate basketLineForUpdate)
    {
        if (!await basketRepository.BasketExists(basketId))
        {
            return NotFound();
        }

        var basketLineEntity = await basketLinesRepository.GetBasketLineById(basketLineId);

        if (basketLineEntity == null)
        {
            return NotFound();
        }

        // map the entity to a dto
        // apply the updated field values to that dto
        // map the dto back to an entity
        basketLineForUpdate.MapToEntity(basketLineEntity);

        basketLinesRepository.UpdateBasketLine(basketLineEntity);
        await basketLinesRepository.SaveChanges();

        return Ok(basketLineEntity.MapToDto());
    }

    [HttpDelete("{basketLineId}")]
    public async Task<IActionResult> Delete(Guid basketId,
        Guid basketLineId)
    {
        if (!await basketRepository.BasketExists(basketId))
        {
            return NotFound();
        }

        var basketLineEntity = await basketLinesRepository.GetBasketLineById(basketLineId);

        if (basketLineEntity == null)
        {
            return NotFound();
        }

        basketLinesRepository.RemoveBasketLine(basketLineEntity);
        await basketLinesRepository.SaveChanges();

        return NoContent();
    }
}
