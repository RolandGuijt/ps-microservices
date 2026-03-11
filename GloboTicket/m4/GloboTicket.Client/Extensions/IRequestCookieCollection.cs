using System;
using GloboTicket.Web.Models;
using Microsoft.AspNetCore.Http;

namespace GloboTicket.Web.Extensions;

public static  class RequestCookieCollection
{
    public static Guid GetCurrentBasketId(this IRequestCookieCollection cookies, Settings settings)
    {
        Guid.TryParse(cookies[settings.BasketIdCookieName], out var basketId);
        return basketId;
    }
    
    public static void RemoveBasket(this IResponseCookies cookies, Settings settings)
    {
        cookies.Delete(settings.BasketIdCookieName);
    }
}
