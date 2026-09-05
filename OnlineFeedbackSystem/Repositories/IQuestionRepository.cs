using OnlineFeedbackSystem.Models.Entities;

namespace OnlineFeedbackSystem.Repositories
{
    public interface IQuestionRepository
    {
        List<Questions> GetAll(string? category = null, string? type = null, string? search = null);
        Questions? GetById(int id);
        int Create(Questions question, List<QuestionOptions> options);
        bool Update(Questions question, List<QuestionOptions> options);
        bool ToggleStatus(int id);
        bool Delete(int id);
        List<string> GetCategories();
    }
}
