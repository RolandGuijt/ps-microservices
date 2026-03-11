using System;
using GloboTicket.Web.Models;
using GloboTicket.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using GrpcShoppingBasketService = GloboTicket.Services.ShoppingBasket.Grpc.ShoppingBasketService;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllersWithViews();

#region addauthn
builder.Services.AddAuthentication(opt =>
    {
        opt.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        opt.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOpenIdConnect(opt =>
    {
        opt.Authority = "https://localhost:5100";

        opt.ClientId = "Web";
        opt.ClientSecret = "3248dsflkjw";

        opt.ResponseType = "code";

        opt.Scope.Add("event-catalog");
        opt.Scope.Add("shopping-basket");
        opt.Scope.Add("order");
        opt.Scope.Add("offline_access");

        opt.GetClaimsFromUserInfoEndpoint = true;

        opt.TokenValidationParameters.NameClaimType = "name";
        opt.TokenValidationParameters.RoleClaimType = "role";

        opt.ClaimActions.MapAll();

        opt.MapInboundClaims = false;

        opt.DisableTelemetry = true;
        opt.SaveTokens = true;
    });
#endregion addauthn

builder.Services.AddHttpClient<IEventCatalogService, EventCatalogService>(c =>
    c.BaseAddress = new Uri("https+http://globoticket-services-eventcatalog"));
builder.Services.AddHttpClient<IOrderService, OrderService>(c =>
    c.BaseAddress = new Uri("https+http://globoticket-services-order"));


var shoppingBasketUrl = builder.Configuration["GLOBOTICKET_SERVICES_SHOPPINGBASKET_HTTPS"] ?? throw new InvalidOperationException();
builder.Services.AddGrpcClient<GrpcShoppingBasketService.ShoppingBasketServiceClient>(o =>
{
    o.Address = new Uri(shoppingBasketUrl);
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