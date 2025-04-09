using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace mailbot_server.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly ApplicationDbContext _dbContext;

    [BindProperty]
    public PaginatedResult<ProcessedEmail> Emails { get; set; } =
        new PaginatedResult<ProcessedEmail>();

    public IndexModel(
        ILogger<IndexModel> logger,
        ApplicationDbContext dbContext
    )
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task OnGetAsync(int pageNumber = 1, int pageSize = 10)
    {
        var paginationInfo = new PaginationInfo()
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
        };
        Emails = await _dbContext
            .ProcessedEmails.OrderByDescending(x => x.Timestamp)
            .GetPaginatedResult<ProcessedEmail>(paginationInfo);
    }
}
