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

        // Pagination Properties
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public int TotalFilteredLogs { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalFilteredLogs / (PageSize > 0 ? PageSize : 15));
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
        public int StartItemIndex => TotalFilteredLogs == 0 ? 0 : ((PageIndex - 1) * PageSize) + 1;
        public int EndItemIndex => Math.Min(PageIndex * PageSize, TotalFilteredLogs);
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
