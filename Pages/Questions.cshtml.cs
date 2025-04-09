using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyApp.Namespace
{
    public class QuestionsModel : PageModel
    {
        private readonly ApplicationDbContext _dbContext;

        [BindProperty]
        public PaginatedResult<QuestionWithAnswer> Items { get; set; } =
            new PaginatedResult<QuestionWithAnswer>();

        public QuestionsModel(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task OnGetAsync(int pageNumber = 1, int pageSize = 10)
        {
            var paginationInfo = new PaginationInfo()
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
            };
            Items = await _dbContext
                .QuestionWithAnswers.OrderByDescending(x => x.Id)
                .GetPaginatedResult<QuestionWithAnswer>(paginationInfo);
        }
    }
}
