using OnlineFeedbackSystem.Models.Entities;

namespace OnlineFeedbackSystem.Models.ViewModels
{
    public class AuditLogIndexViewModel
    {
        public List<AuditLogs> Logs { get; set; } = new();
        public string? SelectedAction { get; set; }
        public string? SelectedEntity { get; set; }
        public string? SearchQuery { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int TotalLogsCount { get; set; }
        public int TodayLogsCount { get; set; }
        public int ActiveUsersCount { get; set; }
        public int FormActionsCount { get; set; }
        public List<string> ActionTypes { get; set; } = new();
        public List<string> EntityTypes { get; set; } = new();
    }

    public class AuditLogStatsDto
    {
        public int TotalLogs { get; set; }
        public int TodayLogs { get; set; }
        public int ActiveUsers { get; set; }
        public int FormActions { get; set; }
        public List<string> ActionTypes { get; set; } = new();
        public List<string> EntityTypes { get; set; } = new();
    }
}
