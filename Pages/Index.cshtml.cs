using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace mailbot_server.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly ApplicationDbContext _dbContext;

    [BindProperty]
    public List<ProcessedEmail> Emails { get; set; } =
        new List<ProcessedEmail>();

    public IndexModel(
        ILogger<IndexModel> logger,
        ApplicationDbContext dbContext
    )
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task OnGetAsync()
    {
        Emails = await _dbContext.ProcessedEmails.ToListAsync();
    }
}
