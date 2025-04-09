public interface IQuestionProvider
{
    Task<IEnumerable<QuestionWithAnswer>> GetAllQuestions();
}
