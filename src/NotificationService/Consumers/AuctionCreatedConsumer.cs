using System;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class AuctionCreatedConsumer(IHubContext<NotificationHub> hubContext) : IConsumer<AuctionCreated>
{
    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine("==> auction created message received");

        //通过 SignalR 把这个消息发给 所有客户端连接，
        //事件名叫 "AuctionCreated"，内容是 context.Message（也就是 AuctionCreated 事件对象）。
        //在前端，只要监听这个事件就能实时更新页面
        await hubContext.Clients.All.SendAsync("AuctionCreated", context.Message);
    }
}
