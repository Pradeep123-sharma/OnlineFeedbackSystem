using OnlineFeedbackSystem.Models.ViewModels;

namespace OnlineFeedbackSystem.Repositories
{
    public interface IResponseReviewRepository
    {
        List<ResponseListItemViewModel> GetResponses(int? formId = null);
        ResponseDetailViewModel? GetResponseDetail(int responseId);
    }
}
