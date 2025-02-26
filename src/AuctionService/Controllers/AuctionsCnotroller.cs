// 使用postman测试api

using System;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
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
    public async Task<ActionResult<List<AuctionDto>>> GetAuctions()
    {
        // 从数据库中的拍卖表中获取数据
        var auctions = await _context.Auctions //auction的类型是List<Auction>
            .Include(x => x.Item) //Include方法预加载(eager loading)关联的Auction.Item数据到内存中。后续使用不需要再执行查询。            
            .OrderBy(x => x.Item.Make)
            .ToListAsync(); //如果你使用了await，就必须调用返回Task类型的方法，例如ToListAsync()

        return _mapper.Map<List<AuctionDto>>(auctions);
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
        var auction = await _context.Auctions.
            Include(x => x.Item).
            FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null)
        {
            return NotFound();
        }

        // TODO: check seller == username

        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

        var result = await _context.SaveChangesAsync() > 0;

        if (result)
        {
            return Ok();
        }

        return BadRequest("Problem saving changes.");
    }
}
