using Microsoft.EntityFrameworkCore;

public static class QueriableExtensions
{
    public static async Task<PaginatedResult<T>> GetPaginatedResult<T>(
        this IQueryable<T> queriable,
        PaginationInfo paginationInfo
    )
    {
        var itemsCount = await queriable.CountAsync();
        var pageItemsCount = paginationInfo.PageSize;

        var pagesDivision = (decimal)itemsCount / pageItemsCount;
        var numberOfPages = (int)Math.Ceiling(pagesDivision);
        var offset = (paginationInfo.PageNumber - 1) * paginationInfo.PageSize;

        var pageResults = await queriable
            .Skip(offset)
            .Take(pageItemsCount)
            .ToListAsync();

        var result = new PaginatedResult<T>()
        {
            Items = pageResults,
            PageNumber = paginationInfo.PageNumber,
            PageCount = numberOfPages,
            TotalCount = itemsCount,
            PageSize = pageItemsCount,
        };

        return result;
    }

    public static PaginatedResult<T> GetPaginatedResult<T>(
        this IEnumerable<T> enumerable,
        PaginationInfo paginationInfo
    )
    {
        var itemsCount = enumerable.Count();
        var pageItemsCount = paginationInfo.PageSize;

        var pagesDivision = (decimal)itemsCount / pageItemsCount;
        var numberOfPages = (int)Math.Ceiling(pagesDivision);
        var offset = (paginationInfo.PageNumber - 1) * paginationInfo.PageSize;

        var pageResults = enumerable.Skip(offset).Take(pageItemsCount).ToList();

        var result = new PaginatedResult<T>()
        {
            Items = pageResults,
            PageNumber = paginationInfo.PageNumber,
            PageCount = numberOfPages,
            TotalCount = itemsCount,
            PageSize = pageItemsCount,
        };

        return result;
    }
}

public class PaginationInfo
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new List<T>();
    public int PageNumber { get; set; }
    public int PageCount { get; set; }
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
}
