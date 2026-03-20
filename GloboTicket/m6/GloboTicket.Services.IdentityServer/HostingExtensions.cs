using Azure.Identity;
using Duende.IdentityServer.Stores;
using Microsoft.AspNetCore.DataProtection;

namespace GloboTicket.Services.IdentityServer;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages();
        
        builder.AddAzureBlobServiceClient("keys-storage");
        //builder.AddAzureKeyVaultClient("identity-keyvault");

        //var keyVaultUri = builder.Configuration.GetConnectionString("identity-keyvault");

        var dpBuilder = builder.Services.AddDataProtection()
            .SetApplicationName("GloboTicket.IdentityServer")
            .PersistKeysToAzureBlobStorage(sp =>
            {
                var client = sp.GetRequiredService<Azure.Storage.Blobs.BlobServiceClient>();
                var container = client.GetBlobContainerClient("identity-keys");
                container.CreateIfNotExists();
                return container.GetBlobClient("dataprotection-keys.xml");
            });

        // if (!string.IsNullOrEmpty(keyVaultUri))
        // {
        //     // Encrypt data protection keys at rest with a Key Vault key
        //     dpBuilder.ProtectKeysWithAzureKeyVault(
        //         new Uri($"{keyVaultUri.TrimEnd('/')}/keys/dataprotection"),
        //         new DefaultAzureCredential());
        // }
        
        builder.Services.AddTransient<ISigningKeyStore, BlobSigningKeyStore>();


        var isBuilder = builder.Services.AddIdentityServer()
            .AddTestUsers(TestUsers.Users)
            .AddLicenseSummary();
        
        // in-memory, code config
        isBuilder.AddInMemoryApiScopes(Config.ApiScopes);
        
        var webClientUrl = builder.Configuration["GLOBOTICKET_WEB_EXTERNAL_HTTPS"]
                           ?? "https://localhost:5000";
        isBuilder.AddInMemoryClients(Config.GetClients(webClientUrl));
        isBuilder.AddInMemoryIdentityResources(Config.IdentityResources);

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {

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
