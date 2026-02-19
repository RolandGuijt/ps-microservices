using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GloboTicket.Services.EventCatalog.Extensions;
using GloboTicket.Services.EventCatalog.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GloboTicket.Services.EventCatalog.Controllers;

[Route("api/events")]
[ApiController]
public class EventController(IEventRepository eventRepository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Models.EventDto>>> Get(
        [FromQuery] Guid categoryId)
    {
        var result = await eventRepository.GetEvents(categoryId);
        return Ok(result.MapToDto());
    }

    [HttpGet("{eventId}")]
    public async Task<ActionResult<Models.EventDto>> GetById(Guid eventId)
    {
        var result = await eventRepository.GetEventById(eventId);
        return Ok(result.MapToDto());
    }
}