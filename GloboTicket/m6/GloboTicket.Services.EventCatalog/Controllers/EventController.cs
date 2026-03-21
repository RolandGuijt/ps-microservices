using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Asp.Versioning;
using GloboTicket.Services.EventCatalog.Extensions;
using GloboTicket.Services.EventCatalog.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GloboTicket.Services.EventCatalog.Controllers;

[Route("api/v{version:apiVersion}/events")]
[ApiController]
[ApiVersion("1.0", Deprecated = true)]
[ApiVersion("2.0")]
public class EventController(IEventRepository eventRepository) : ControllerBase
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<IEnumerable<Models.EventDto>>> GetV1(
        [FromQuery] Guid categoryId)
    {
        var result = await eventRepository.GetEvents(categoryId);
        return Ok(result.MapToDto());
    }
    
    [HttpGet]
    [MapToApiVersion("2.0")]
    public async Task<ActionResult<IEnumerable<Models.EventDto>>> GetV2(
        [FromQuery] Guid categoryId)
    {
        var result = await eventRepository.GetEvents(categoryId);
        return Ok(result.MapToDto());
    }


    [HttpGet("{eventId}")]
    [MapToApiVersion("1.0")]
    [MapToApiVersion("2.0")]
    public async Task<ActionResult<Models.EventDto>> GetById(Guid eventId)
    {
        var result = await eventRepository.GetEventById(eventId);
        return Ok(result.MapToDto());
    }
}