using OnlineFeedbackSystem.Models.ViewModels;

namespace OnlineFeedbackSystem.Repositories
{
    public interface ISuperAdminDashboardRepository
    {
        DashboardStatsViewModel GetStats();
    }
}