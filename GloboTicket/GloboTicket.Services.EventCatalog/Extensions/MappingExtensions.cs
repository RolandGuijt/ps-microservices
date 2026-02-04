using System.Collections.Generic;
using System.Linq;
using GloboTicket.Services.EventCatalog.Entities;
using GloboTicket.Services.EventCatalog.Models;

namespace GloboTicket.Services.EventCatalog.Extensions;

public static class MappingExtensions
{
    public static CategoryDto MapToDto(this Category category)
    {
        if (category == null) return null;
        return new CategoryDto
        {
            CategoryId = category.CategoryId,
            Name = category.Name
        };
    }

    public static List<CategoryDto> MapToDto(this IEnumerable<Category> categories)
    {
        return categories?.Select(c => c.MapToDto()).ToList() ?? new List<CategoryDto>();
    }

    public static EventDto MapToDto(this Event @event)
    {
        if (@event == null) return null;
        return new EventDto
        {
            EventId = @event.EventId,
            Name = @event.Name,
            Price = @event.Price,
            Artist = @event.Artist,
            Date = @event.Date,
            Description = @event.Description,
            ImageUrl = @event.ImageUrl,
            CategoryId = @event.CategoryId,
            CategoryName = @event.Category?.Name
        };
    }

    public static List<EventDto> MapToDto(this IEnumerable<Event> events)
    {
        return events?.Select(e => e.MapToDto()).ToList() ?? new List<EventDto>();
    }
}
