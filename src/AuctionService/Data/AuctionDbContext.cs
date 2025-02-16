using System;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

public class AuctionDbContext : DbContext
{
    // DbContext 没有无参构造函数，所以 AuctionDbContext 必须手动调用 base(options) 来初始化 DbContext。
    public AuctionDbContext(DbContextOptions options) : base(options)
    {

    }
}


