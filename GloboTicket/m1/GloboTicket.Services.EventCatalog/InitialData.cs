using System;
using System.Linq;
using GloboTicket.Services.EventCatalog.DbContexts;
using GloboTicket.Services.EventCatalog.Entities;
using Microsoft.EntityFrameworkCore;

namespace GloboTicket.Services.EventCatalog;

public static class SampleData
{
    public static void GetSeedData(DbContext context)
    {
        if (!context.Set<Category>().Any())
        {
            context.Set<Category>().Add(new Category
            {
                CategoryId = new Guid("cfb88e29-4744-48c0-94fa-b25b92dea314"),
                Name = "Concerts"
            });
            context.Set<Category>().Add(new Category
            {
                CategoryId = new Guid("cfb88e29-4744-48c0-94fa-b25b92dea315"),
                Name = "Musicals"
            });
            context.Set<Category>().Add(new Category
            {
                CategoryId = new Guid("cfb88e29-4744-48c0-94fa-b25b92dea316"),
                Name = "Plays"
            });
        }

        if (!context.Set<Event>().Any())
        {
            context.Set<Event>().Add(new Event
            {
                EventId = new Guid("cfb88e29-4744-48c0-94fa-b25b92dea317"),
                Artist = "John Egbert",
                CategoryId = new Guid("cfb88e29-4744-48c0-94fa-b25b92dea314"),
                Date = new DateTime(2027, 1, 11, 15, 0, 0).ToUniversalTime(),
                Description =
                    "Join John for his farwell tour across 15 continents. John really needs no introduction since he has already mesmerized the world with his banjo.",
                ImageUrl = "/img/banjo.jpg",
                Name = "John Egbert Live",
                Price = 65
            });

            context.Set<Event>().Add(new Event
            {
                EventId = new Guid("cfb88e29-4744-48c0-94fa-b25b92dea319"),
                Artist = "Michael Johnson",
                CategoryId = new Guid("cfb88e29-4744-48c0-94fa-b25b92dea314"),
                Date = new DateTime(2027, 5, 11, 11, 0, 0).ToUniversalTime(),
                Description =
                    "Michael Johnson doesn't need an introduction. His 25 concert across the globe last year were seen by thousands. Can we add you to the list?",
                ImageUrl = "/img/michael.jpg",
                Name = "The State of Affairs: Michael Live!",
                Price = 85
            });

            context.Set<Event>().Add(new Event
            {
                EventId = new Guid("cfb88e29-4744-48c0-94fa-b25b92dea318"),
                Artist = "Nick Sailor",
                CategoryId = new Guid("cfb88e29-4744-48c0-94fa-b25b92dea315"),
                Date = new DateTime(2027, 9, 11, 1, 0, 0).ToUniversalTime(),
                Description =
                    "The critics are over the moon and so will you after you've watched this sing and dance extravaganza written by Nick Sailor, the man from 'My dad and sister'.",
                ImageUrl = "/img/musical.jpg",
                Name = "To the Moon and Back",
                Price = 135
            });
        }
    }
}