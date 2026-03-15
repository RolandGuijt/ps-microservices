using GloboTicket.Web.Models;
using GloboTicket.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using GloboTicket.Web.Areas.Order.ViewModels;

namespace GloboTicket.Web.Controllers
{
    [Area("Order")]
    public class OrderController(Settings settings, IOrderService orderService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var orders = await orderService.GetOrdersForUser(settings.UserId);

            return View(new OrderViewModel { Orders = orders });
        }
    }
}
