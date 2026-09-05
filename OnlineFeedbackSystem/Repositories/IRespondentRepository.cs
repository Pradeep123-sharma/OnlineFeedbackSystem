using OnlineFeedbackSystem.Models.Entities;
using OnlineFeedbackSystem.Models.ViewModels;

namespace OnlineFeedbackSystem.Repositories
{
    public interface IRespondentRepository
    {
        List<RespondentAvailableFormItemViewModel> GetAvailableForms(int? userId = null);
        RespondentFormFillingViewModel? GetFormForFilling(int formId);
        bool HasUserSubmitted(int formId, int userId);
        string SubmitFeedback(int formId, int? userId, List<RespondentQuestionInputViewModel> answers);
        RespondentDashboardViewModel GetRespondentDashboard(int? userId, string? userName = null);
        List<RespondentSubmissionHistoryItemViewModel> GetUserSubmissions(int userId);
    }
}
