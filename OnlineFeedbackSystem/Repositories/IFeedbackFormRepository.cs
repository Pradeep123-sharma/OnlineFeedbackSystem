using OnlineFeedbackSystem.Models.Entities;
using OnlineFeedbackSystem.Models.ViewModels;

namespace OnlineFeedbackSystem.Repositories
{
    public interface IFeedbackFormRepository
    {
        List<FeedbackFormListItemViewModel> GetAll(string? status = null, string? category = null, string? search = null, int? departmentId = null, int? createdBy = null);
        FeedbackForms? GetById(int id);
        int Create(FeedbackForms form);
        bool Update(FeedbackForms form);
        bool ChangeStatus(int id, string status);
        bool Delete(int id);
        List<FormQuestionItemViewModel> GetFormQuestions(int formId);
        int AddFormQuestion(int formId, int questionId, bool isRequired = true, int? displayOrder = null);
        bool RemoveFormQuestion(int formId, int questionId);
        bool ReorderFormQuestion(int formQuestionId, int displayOrder, bool isRequired);
        bool ReorderAllQuestions(int formId, List<int> orderedQuestionIds);
        bool MoveQuestion(int formId, int questionId, string direction);
        bool UpdateFormQuestionRequired(int formId, int questionId, bool isRequired);
        List<string> GetCategories();
    }
}
