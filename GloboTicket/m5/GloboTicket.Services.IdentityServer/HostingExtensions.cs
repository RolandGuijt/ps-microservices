using System.Globalization;
using Duende.IdentityServer;
using GloboTicket.Services.IdentityServer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Filters;

namespace GloboTicket.Services.IdentityServer;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages();

        var isBuilder = builder.Services.AddIdentityServer()
            .AddTestUsers(TestUsers.Users)
            .AddLicenseSummary();
        
        // in-memory, code config
        isBuilder.AddInMemoryApiScopes(Config.ApiScopes);
        isBuilder.AddInMemoryClients(Config.Clients);
        isBuilder.AddInMemoryIdentityResources(Config.IdentityResources);

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.MapDefaultEndpoints();

        app.UseStaticFiles();
        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();

        app.MapRazorPages()
            .RequireAuthorization();

        return app;
    }
}
