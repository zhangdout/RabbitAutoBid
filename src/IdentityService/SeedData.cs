using System.Security.Claims;
using IdentityModel;
using IdentityService.Data;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IdentityService;

// 这段代码的作用是 初始化 Identity Server 的用户数据，确保 alice 和 bob 这两个用户在数据库中存在，并给他们添加了一些身份声明（Claims）。
public class SeedData
{
    public static void EnsureSeedData(WebApplication app)
    {
        /*
        创建一个新的 "作用域"（Scope），用于获取 ApplicationDbContext 和 UserManager<ApplicationUser>。
        确保数据库上下文（DbContext）在方法执行完后被正确释放，避免资源泄露。
        🔍 为什么要创建作用域（Scope）？
        **ASP.NET Core 依赖注入（DI）中，DbContext 是 **"Scoped"（作用域范围）的服务。

        ApplicationDbContext 是一个数据库连接实例，每次 HTTP 请求都会创建一个新的实例。
        如果在 EnsureSeedData() 里直接用 app.Services 获取 ApplicationDbContext，可能会导致资源冲突。
        UserManager<ApplicationUser> 依赖 DbContext

        UserManager<T> 依赖于 ApplicationDbContext 来查询和管理用户信息。
        必须在相同的作用域（Scope）里获取 DbContext 和 UserManager，否则 UserManager 可能会出错。
        */
        using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            //UserManager<ApplicationUser> 用于 管理 ASP.NET Identity 用户（创建、查找、删除、分配角色等）。
            //Identity Server 使用 ASP.NET Identity 作为用户存储。
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.Migrate();

            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            if(userMgr.Users.Any()) return;

            var alice = userMgr.FindByNameAsync("alice").Result;
            if (alice == null)
            {
                alice = new ApplicationUser
                {
                    UserName = "alice",
                    Email = "AliceSmith@email.com",
                    EmailConfirmed = true,
                };
                var result = userMgr.CreateAsync(alice, "Pass123$").Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = userMgr.AddClaimsAsync(alice, new Claim[]{
                            new Claim(JwtClaimTypes.Name, "Alice Smith")
                        }).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
                Log.Debug("alice created");
            }
            else
            {
                Log.Debug("alice already exists");
            }

            var bob = userMgr.FindByNameAsync("bob").Result;
            if (bob == null)
            {
                bob = new ApplicationUser
                {
                    UserName = "bob",
                    Email = "BobSmith@email.com",
                    EmailConfirmed = true
                };
                var result = userMgr.CreateAsync(bob, "Pass123$").Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = userMgr.AddClaimsAsync(bob, new Claim[]{
                            new Claim(JwtClaimTypes.Name, "Bob Smith")
                        }).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
                Log.Debug("bob created");
            }
            else
            {
                Log.Debug("bob already exists");
            }
        }
    }
}
