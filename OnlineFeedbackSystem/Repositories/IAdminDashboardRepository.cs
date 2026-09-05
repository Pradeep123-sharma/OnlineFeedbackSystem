using OnlineFeedbackSystem.Models.ViewModels;

namespace OnlineFeedbackSystem.Repositories
{
    public interface IAdminDashboardRepository
    {
        AdminDashboardViewModel GetDashboardStats(int? createdBy = null);
    }
}
