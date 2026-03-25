using System;
using Duende.AccessTokenManagement.OpenIdConnect;
using GloboTicket.Web.Models;
using GloboTicket.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using GrpcShoppingBasketService = GloboTicket.Services.ShoppingBasket.Grpc.ShoppingBasketService;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(opt =>
    {
        opt.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        opt.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOpenIdConnect(opt =>
    {
        opt.Authority = builder.Configuration["GLOBOTICKET_SERVICES_IDENTITY_HTTPS"];

        opt.ClientId = "Web";
        opt.ClientSecret = "3248dsflkjw";

        opt.ResponseType = "code";

        opt.Scope.Add("event-catalog");
        opt.Scope.Add("shopping-basket");
        opt.Scope.Add("order");
        opt.Scope.Add("offline_access");

        opt.TokenValidationParameters.NameClaimType = "name";
        opt.TokenValidationParameters.RoleClaimType = "role";

        opt.ClaimActions.MapAll();

        opt.MapInboundClaims = false;
        opt.DisableTelemetry = true;
        opt.SaveTokens = true;
    });

builder.Services.AddOpenIdConnectAccessTokenManagement();

builder.Services.AddUserAccessTokenHttpClient("event-catalog-client", 
    configureClient:c =>
        c.BaseAddress = new Uri("https+http://globoticket-services-eventcatalog"));
builder.Services.AddUserAccessTokenHttpClient("order-client", 
    configureClient:c =>
        c.BaseAddress = new Uri("https+http://globoticket-services-order"));


var shoppingBasketUrl = builder.Configuration["GLOBOTICKET_SERVICES_SHOPPINGBASKET_HTTPS"] ?? throw new InvalidOperationException();
builder.Services.AddGrpcClient<GrpcShoppingBasketService.ShoppingBasketServiceClient>(o =>
{
    o.Address = new Uri(shoppingBasketUrl);
}).AddUserAccessTokenHandler();

builder.Services.AddScoped<IEventCatalogService, EventCatalogService>();
builder.Services.AddScoped<IOrderService, OrderService>();
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
        pattern: "{area=EventCatalog}/{controller=EventCatalog}/{action=Index}/{id?}")
    .RequireAuthorization();

app.Run();