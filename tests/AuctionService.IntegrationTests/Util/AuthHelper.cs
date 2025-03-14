using System;
using System.Security.Claims;

namespace AuctionService.IntegrationTests.Util;

public class AuthHelper
{
    //生成一个包含 username 作为 ClaimTypes.Name 的 Dictionary，可以用来创建 身份验证的 Bearer 令牌。
    public static Dictionary<string, object> GetBearerForUser(string username)
    {
        return new Dictionary<string, object>{{ClaimTypes.Name, username}};
    }
}
