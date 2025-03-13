using System;
using System.Security.Claims;

namespace AuctionService.UnitTests.Utils;

public class Helpers
{
    //这段代码的目的是创建一个 ClaimsPrincipal 对象，用于模拟用户身份，通常用于单元测试时测试身份验证相关的功能。
    public static ClaimsPrincipal GetClaimsPrincipal()
    {
        var claims = new List<Claim>{new Claim(ClaimTypes.Name, "test")};
        var identity = new ClaimsIdentity(claims, "testing");
        return new ClaimsPrincipal(identity);
    }
}
