using System;

namespace SearchService.RequestHelpers;

// 包含了search httpget方法中所有用到的参数，参数比较多，放入一个对象便于管理
public class SearchParams
{
    public string SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 4;
    public string Seller { get; set; }
    public string Winner { get; set; }
    public string OrderBy { get; set; }
    public string FilterBy { get; set; }
}
