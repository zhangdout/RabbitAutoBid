using System;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    // searchTerm 是从 URL 查询参数 传入的搜索关键字，例如 ?searchTerm=Toyota
    [HttpGet]
    //【APIController】默认从body获取参数。要想从查询参数重获取参数，必须注解【Fromquery】
    public async Task<ActionResult<List<Item>>> SearchItems([FromQuery]SearchParams searchParams)
    {
        //DB.Find<Item>() 不会立即查询数据库，而是创建了一个查询对象 query。
        //这个 query 只是一个查询构造器（Query Builder），它存储了查询的各种条件（比如过滤、排序等），但还没有真正去数据库执行查询。
        //相当于 你打开了一个数据库查询工具，准备输入 SQL 语句，但还没有按回车键执行查询。
        //改成了分页查询。
        var query = DB.PagedSearch<Item>();

        //query.Sort(...) 并不会立即对数据库进行排序，而是在 query 这个查询对象上 添加排序条件。

        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
        }

        switch (searchParams.OrderBy)
        {
            case "make":
                query.Sort(x => x.Ascending(a => a.Make));
                break;
            case "new":
                query.Sort(x => x.Descending(a => a.CreatedAt));
                break;
            default:
                query.Sort(x => x.Ascending(a => a.AuctionEnd));
                break;
        }

        switch (searchParams.FilterBy)
        {
            case "finished":
                query.Match(x => x.AuctionEnd > DateTime.UtcNow);
                break;
            case "endingSoon":
                query.Match(x => x.AuctionEnd > DateTime.UtcNow.AddHours(6) && x.AuctionEnd < DateTime.UtcNow);
                break;
            default:
                query.Match(x => x.AuctionEnd > DateTime.UtcNow);
                break;
        }

        if (!string.IsNullOrEmpty(searchParams.Seller))
        {
            query.Match(x => x.Seller == searchParams.Seller);
        }

        if (!string.IsNullOrEmpty(searchParams.Winner))
        {
            query.Match(x => x.Winner == searchParams.Winner);
        }

        query.PageNumber(searchParams.PageNumber);
        query.PageSize(searchParams.PageSize);

        //直到你调用 query.ExecuteAsync();，MongoDB 才真正执行查询。并应用所有之前添加的条件（包括 Sort）。
        var result = await query.ExecuteAsync();

        return Ok(new
        {
            results = result.Results,
            pageCount = result.PageCount,
            totalCount = result.TotalCount
        });
    }
}
