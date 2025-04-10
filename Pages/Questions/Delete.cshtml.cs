using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace mailbot_server.Pages.Questions
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
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

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var questionwithanswer =
                await _context.QuestionWithAnswers.FindAsync(id);
            if (questionwithanswer != null)
            {
                QuestionWithAnswer = questionwithanswer;
                _context.QuestionWithAnswers.Remove(QuestionWithAnswer);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
