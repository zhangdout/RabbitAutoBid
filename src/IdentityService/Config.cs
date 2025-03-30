using Duende.IdentityServer.Models;

namespace IdentityService;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            //openid scope
            new IdentityResources.OpenId(),
            //profile scope
            new IdentityResources.Profile(),
        };

    //ApiScopes（API 作用域）用于定义客户端（Client）可以访问的 API 权限。控制 OAuth 2.0 访问令牌（Access Token）可以访问哪些 API 资源。
    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("auctionApp", "Auction app full acess")
        };

    public static IEnumerable<Client> Clients(IConfiguration config) =>
        new Client[]
        {
            new Client
            {
                ClientId = "postman",
                ClientName = "Postman",
                //当客户端（Client）请求访问 API 时，它会向 IdentityServer 请求 Access Token。
                //如果身份服务器同意，返回的 Access Token（JWT）会包含AllowedScopes
                //在 ASP.NET Core 的 API 端，我们需要解析 Access Token，然后验证作用域。
                //你可以在 API 控制器（Controller）里使用 [Authorize] 来限制访问权限。
                AllowedScopes = {"openid", "profile", "auctionApp"},
                RedirectUris = {"https://www.getpostman.com/oauth2/callback"},
                ClientSecrets = new[] {new Secret("NotASecret".Sha256())},
                AllowedGrantTypes = {GrantType.ResourceOwnerPassword}
            },
            new Client
            {
                ClientId = "nextApp",
                ClientName = "nextApp",
                ClientSecrets = new[] {new Secret("secret".Sha256())},
                AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                RequirePkce = false,
                RedirectUris = {config["ClientApp"] + "/api/auth/callback/id-server"},
                AllowOfflineAccess = true,
                AllowedScopes = {"openid", "profile", "auctionApp"},
                AccessTokenLifetime = 3600*24*30,
                AlwaysIncludeUserClaimsInIdToken = true
            }
        };
}
