using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        //API 会验证 Token 是否由 IdentityServiceUrl的地址 签发
        options.Authority = builder.Configuration["IdentityServiceUrl"];
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters.ValidateAudience = false;
        options.TokenValidationParameters.NameClaimType = "username";
    });

//在大多数情况下，**如果你使用 Next.js 的 Node.js 服务器作为中间层 向 ASP.NET Core 后端发请求，通常不需要配置 CORS。
//只有当浏览器的前端代码直接向“跨域”的服务器发请求时，才触发 CORS。
//SignalR 通过 HTTP 协议建立连接（WebSocket、SSE、Long Polling），所以它会受到浏览器的跨域限制，需要服务端允许跨域。
//前端和后端不在同一个域名/端口下运行时，此时如果前端尝试连接 SignalR，浏览器会发一个 OPTIONS 预检请求，结果如果服务端没配置 CORS，就会报错
builder.Services.AddCors(options =>
{
    options.AddPolicy("customPolicy", b =>
    {
        b.AllowAnyHeader().AllowAnyMethod().AllowCredentials()
            .WithOrigins(builder.Configuration["ClientApp"]!);
    });
});

var app = builder.Build();

app.UseCors();

app.MapReverseProxy();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
