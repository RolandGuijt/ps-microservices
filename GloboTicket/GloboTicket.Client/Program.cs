using System;
using GloboTicket.Web.Models;
using GloboTicket.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient<IEventCatalogService, EventCatalogService>(c => 
    c.BaseAddress = new Uri(builder.Configuration["ApiConfigs:EventCatalog:Uri"] ?? throw new InvalidOperationException()));
builder.Services.AddHttpClient<IShoppingBasketService, ShoppingBasketService>(c => 
    c.BaseAddress = new Uri(builder.Configuration["ApiConfigs:ShoppingBasket:Uri"] ?? throw new InvalidOperationException()));

builder.Services.AddSingleton<Settings>();

var app = builder.Build();

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
        pattern: "{controller=EventCatalog}/{action=Index}/{id?}");

app.Run();