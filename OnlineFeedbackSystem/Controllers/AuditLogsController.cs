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
        public IActionResult Index(
            string? actionType,
            string? entityName,
            string? search,
            DateTime? startDate,
            DateTime? endDate,
            int page = 1,
            int pageSize = 15)
        {
            if (page < 1) page = 1;
            if (pageSize < 5) pageSize = 15;
            if (pageSize > 100) pageSize = 100;

            var allLogs = _auditLogRepository.GetLogs(actionType, entityName, search, startDate, endDate, top: 1000);
            var stats = _auditLogRepository.GetStats();

            int totalFiltered = allLogs.Count;
            var pagedLogs = allLogs.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var viewModel = new AuditLogIndexViewModel
            {
                Logs = pagedLogs,
                SelectedAction = actionType,
                SelectedEntity = entityName,
                SearchQuery = search,
                StartDate = startDate,
                EndDate = endDate,
                PageIndex = page,
                PageSize = pageSize,
                TotalFilteredLogs = totalFiltered,
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
