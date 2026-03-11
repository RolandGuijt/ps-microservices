using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GloboTicket.Web.Areas.Order.Models;

namespace GloboTicket.Web.Services
{
    public interface IOrderService
    {
        Task<List<Order>> GetOrdersForUser(Guid userId);
    }
}
