using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace mailbot_server.Pages.Questions
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public QuestionWithAnswer QuestionWithAnswer { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var questionwithanswer =
                await _context.QuestionWithAnswers.FirstOrDefaultAsync(m =>
                    m.Id == id
                );

            if (questionwithanswer is not null)
            {
                QuestionWithAnswer = questionwithanswer;

                return Page();
            }

            return NotFound();
        }
    }
}
