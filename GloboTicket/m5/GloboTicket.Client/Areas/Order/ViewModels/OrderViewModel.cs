using System.Collections.Generic;

namespace GloboTicket.Web.Areas.Order.ViewModels;

public class OrderViewModel
{
    public List<Models.Order> Orders { get; set; }
}