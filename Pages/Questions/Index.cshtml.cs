using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace mailbot_server.Pages.Questions
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<QuestionWithAnswer> QuestionWithAnswer { get; set; } =
            default!;

        public async Task OnGetAsync()
        {
            QuestionWithAnswer =
                await _context.QuestionWithAnswers.ToListAsync();
        }
    }
}
