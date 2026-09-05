using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineFeedbackSystem.Models.ViewModels;
using OnlineFeedbackSystem.Repositories;

namespace OnlineFeedbackSystem.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class AuditLogsController : Controller
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditLogsController(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        [HttpGet]
        public IActionResult Index(string? actionType, string? entityName, string? search, DateTime? startDate, DateTime? endDate)
        {
            var logs = _auditLogRepository.GetLogs(actionType, entityName, search, startDate, endDate);
            var stats = _auditLogRepository.GetStats();

            var viewModel = new AuditLogIndexViewModel
            {
                Logs = logs,
                SelectedAction = actionType,
                SelectedEntity = entityName,
                SearchQuery = search,
                StartDate = startDate,
                EndDate = endDate,
                TotalLogsCount = stats.TotalLogs,
                TodayLogsCount = stats.TodayLogs,
                ActiveUsersCount = stats.ActiveUsers,
                FormActionsCount = stats.FormActions,
                ActionTypes = stats.ActionTypes,
                EntityTypes = stats.EntityTypes
            };

            return View("~/Views/SuperAdmin/AuditLogs.cshtml", viewModel);
        }
    }
}
