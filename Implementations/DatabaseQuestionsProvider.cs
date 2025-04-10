using Microsoft.EntityFrameworkCore;

public class DatabaseQuestionsProvider : IQuestionProvider
{
    private readonly ApplicationDbContext _dbContext;

    public DatabaseQuestionsProvider(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<QuestionWithAnswer>> GetAllQuestions()
    {
        return await _dbContext.QuestionWithAnswers.ToListAsync();
    }
}
