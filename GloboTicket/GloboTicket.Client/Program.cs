using System;
using System.Net.Http;
using System.Threading;
using GloboTicket.Web.Models;
using GloboTicket.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using GrpcShoppingBasketService = GloboTicket.Services.ShoppingBasket.Grpc.ShoppingBasketService;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient<IEventCatalogService, EventCatalogService>(c =>
    c.BaseAddress = new Uri("https+http://globoticket-services-eventcatalog"));

var shoppingBasketUrl = builder.Configuration["GLOBOTICKET_SERVICES_SHOPPINGBASKET_HTTPS"] ?? throw new InvalidOperationException();
builder.Services.AddGrpcClient<GrpcShoppingBasketService.ShoppingBasketServiceClient>(o =>
    {
        o.Address = new Uri(shoppingBasketUrl);
    })
    .ConfigureChannel(channel =>
    {
        channel.HttpHandler = new SocketsHttpHandler
        {
            PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
            KeepAlivePingDelay = TimeSpan.FromSeconds(60),
            KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
            EnableMultipleHttp2Connections = true
        };
    });

builder.Services.AddScoped<IShoppingBasketService, ShoppingBasketService>();

builder.Services.AddSingleton<Settings>();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
        name: "default",
        pattern: "{area=EventCatalog}/{controller=EventCatalog}/{action=Index}/{id?}");

app.Run();