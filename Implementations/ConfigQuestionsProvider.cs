public class ConfigQuestionsProvider : IQuestionProvider
{
    private readonly IConfiguration _configuration;

    public ConfigQuestionsProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<IEnumerable<QuestionWithAnswer>> GetAllQuestions()
    {
        await Task.CompletedTask;

        var result =
            _configuration
                .GetSection("ChatGpt:Questions")
                .Get<List<QuestionWithAnswer>>()
            ?? new List<QuestionWithAnswer>();

        return result;
    }
}
