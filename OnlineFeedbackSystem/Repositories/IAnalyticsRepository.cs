using OnlineFeedbackSystem.Models.ViewModels;

namespace OnlineFeedbackSystem.Repositories
{
    public interface IAnalyticsRepository
    {
        FormAnalyticsViewModel? GetFormAnalytics(int formId);
    }
}
