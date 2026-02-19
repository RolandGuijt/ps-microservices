using System;
using System.Threading.Tasks;
using GloboTicket.Web.Extensions;
using GloboTicket.Web.Models;
using GloboTicket.Web.Models.Api;
using GloboTicket.Web.Models.View;
using GloboTicket.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace GloboTicket.Web.Areas.EventCatalog.Controllers;

[Area("EventCatalog")]
public class EventCatalogController(IEventCatalogService eventCatalogService, 
    Settings settings) : Controller
{

    public async Task<IActionResult> Index(Guid categoryId)
    {
        var currentBasketId = Request.Cookies.GetCurrentBasketId(settings);
        
        var getCategories = eventCatalogService.GetCategories();
        var getEvents = categoryId == Guid.Empty ? eventCatalogService.GetAll() :
            eventCatalogService.GetByCategoryId(categoryId);
        await Task.WhenAll([getCategories, getEvents]);

        return View(
            new EventListModel
            {
                Events = getEvents.Result,
                Categories = getCategories.Result,
                NumberOfItems = 0
                ,
                SelectedCategory = categoryId
            }
        );
    }

    [HttpPost]
    public IActionResult SelectCategory([FromForm]Guid selectedCategory)
    {
        return RedirectToAction("Index", new { categoryId = selectedCategory });
    }

    public async Task<IActionResult> Detail(Guid eventId)
    {
        var ev = await eventCatalogService.GetEvent(eventId);
        return View(ev);
    }
}
