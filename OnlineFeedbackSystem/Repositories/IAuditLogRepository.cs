using OnlineFeedbackSystem.Models.Entities;
using OnlineFeedbackSystem.Models.ViewModels;

namespace OnlineFeedbackSystem.Repositories
{
    public interface IAuditLogRepository
    {
        List<AuditLogs> GetLogs(string? action = null, string? entityName = null, string? search = null, DateTime? startDate = null, DateTime? endDate = null, int top = 200);
        AuditLogStatsDto GetStats();
        int InsertLog(int? userId, string action, string? entityName = null, int? entityId = null, string? details = null);
    }
}
