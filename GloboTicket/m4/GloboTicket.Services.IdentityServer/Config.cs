using Duende.IdentityModel;
using Duende.IdentityServer.Models;

namespace GloboTicket.Services.IdentityServer;

public static class Config
{
    #region identityresources
    public static IEnumerable<IdentityResource> IdentityResources =>
    [
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
    ];
    #endregion

    public static IEnumerable<ApiScope> ApiScopes =>
        [
            new ApiScope("event-catalog"),
            new ApiScope("shopping-basket"),
            new ApiScope("order"),
        ];

    public static IEnumerable<Client> Clients =>
        [
            new()
            {
                ClientId = "Web",
                ClientSecrets = { new("3248dsflkjw".Sha256()) },
                AllowedGrantTypes = GrantTypes.Code,

                RedirectUris = { "https://localhost:5000/signin-oidc" },
                AllowedScopes = { "openid", "profile", "event-catalog", "shopping-basket", "order" },

                AllowOfflineAccess = true,
            },
            new()
            {
                ClientId = "ShoppingBasket",
                ClientSecrets = { new("wexite43".Sha256()) },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                
                AllowedScopes = { "event-catalog" },
            }
        ];
}
