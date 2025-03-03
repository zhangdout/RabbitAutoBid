// 使用postman测试api

using System;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

/*
[ApiController] -- 数据注解Data Annotations
自动验证 ModelState（不需要手动 if (!ModelState.IsValid)）。
自动从 Body 绑定复杂类型（例如 UpdateAuctionDto）。
自动从 Route 绑定 id（如果 HttpPut("{id}") 匹配）。
自动返回 400 错误（如果 DTO 绑定失败）。

可能导致 400 Bad Request 的情况：
请求头 Content-Type 不正确。如果客户端发送的数据不是 application/json，会导致 400
DTO 绑定失败：如果 UpdateAuctionDto 有 [Required] 修饰的字段，缺少字段会导致 400 Bad Request
*/

[ApiController]
[Route("api/auctions")]
//Controller 是 Transient，每次请求创建新的实例
public class AuctionsController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;

    public AuctionsController(AuctionDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAuctions(string date)
    {
        //.AsQueryable() → 转换为 IQueryable<Auction>，用于动态查询。
        //IQueryable<T> 是 C# 延迟执行（Lazy Execution）的查询对象，它允许构造复杂查询，而不会立即执行数据库查询。
        //只有在最终调用 .ToList(), .FirstOrDefault(), .Count() 等方法时，查询才会真正执行。
        var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

        if (!string.IsNullOrEmpty(date))
        {
            query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
        }

        /*
        // 从数据库中的拍卖表中获取数据
        var auctions = await _context.Auctions //auction的类型是List<Auction>
            .Include(x => x.Item) //Include方法预加载(eager loading)关联的Auction.Item数据到内存中。后续使用不需要再执行查询。            
            .OrderBy(x => x.Item.Make)
            .ToListAsync(); //如果你使用了await，就必须调用返回Task类型的方法，例如ToListAsync()

        return _mapper.Map<List<AuctionDto>>(auctions);
        */

        //ProjectTo<AuctionDto>（）用 AutoMapper 把 IQueryable<Auction> 转换成 IQueryable<AuctionDto>
        //query 的类型是 IQueryable<Auction>，因为：_context.Auctions 是 DbSet<Auction>，默认就是 IQueryable<Auction>
        //只有 ToListAsync() 被调用时，查询才会真正执行
        //在 AutoMapper 中，_mapper.ConfigurationProvider 不直接存储 MappingProfile，但它包含了 MappingProfile 注册的所有映射规则。
        return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null)
        {
            return NotFound();
        }

        return _mapper.Map<AuctionDto>(auction);
    }

    [HttpPost]
    // 如果不返回数据，只返回状态码，使用Task<IActionResult>更灵活
    // 参数CreateAuctionDto auctionDto来自于客户端发来的http post请求体
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);

        // TODO: add current user as seller
        auction.Seller = "test";

        _context.Auctions.Add(auction);
        // Add 方法仅在内存中标记对象状态为待新增（Added），并不会立即执行数据库操作
        // SaveChanges 方法才会真正执行数据库命令（如INSERT）持久化数据到数据库中。
        /*
            这是 EF Core 中常见的设计模式：Unit of Work（工作单元）模式。
            允许批量提交多个更改，一次性地完成所有数据库操作。
            提高数据库性能、事务管理以及代码控制的灵活性。
        */
        var result = await _context.SaveChangesAsync() > 0;
        if (!result)
        {
            return BadRequest("Could not save changes to the DB.");
        }
        // CreatedAtAction方法返回状态码201 Created，同时可以返回数据给客户端。这里是客户创建的Auction转换成AuctionDto，返回给客户查看这个数据。
        // CreatedAtAction方法第一个参数是string，告诉客户端用于生成新创建资源URL的方法名。第二个参数是object，表示生成URL所需的路由参数。第三个参数是返回的数据，常返回新创建的对象本身。
        // 第二个参数的属性名必须与你指定的Action方法（这里指GetAuctionById）的参数名一致，即id（大小写必须区分）。
        return CreatedAtAction(nameof(GetAuctionById), new { ID = auction.Id }, _mapper.Map<AuctionDto>(auction));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        // 这个查询会在内存中创建 auction 对象，并且 EF Core 会开始**跟踪（Tracking）**这个对象。

        var auction = await _context.Auctions.
            Include(x => x.Item).
            FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null)
        {
            return NotFound();
        }

        // TODO: check seller == username

        // 这里修改了 Item 的 Make 属性，但EF Core 仍然在跟踪 auction，它知道 Make 被修改了。

        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

        // EF Core 自动跟踪（Tracking） 被查询出来的实体，当它的属性被修改时，EF Core 会记录哪些字段被更改，然后在 SaveChangesAsync() 时自动更新到数据库。

        // SaveChangesAsync() 会检查所有被 EF Core 跟踪的对象，如果发现有字段被修改，它就会自动生成 UPDATE SQL 语句并执行。

        var result = await _context.SaveChangesAsync() > 0;

        if (result)
        {
            return Ok();
        }

        return BadRequest("Problem saving changes.");
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions.FindAsync(id);

        if (auction == null)
        {
            return NotFound();
        }

        // 这行代码的作用是将 auction 标记为 "Deleted" 状态，并不会立即从数据库中删除数据，而是等待 SaveChangesAsync() 时真正执行 DELETE 语句。

        _context.Auctions.Remove(auction);

        // 执行 await _context.SaveChangesAsync(); 之后，EF Core 执行的 SQL 语句类似于：DELETE FROM Auctions WHERE Id = 'some-guid-id';

        var result = await _context.SaveChangesAsync() > 0;

        if (result)
        {
            return Ok();
        }

        return BadRequest("Could not update DB.");
    }
}
