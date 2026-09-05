using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineFeedbackSystem.Repositories;

namespace OnlineFeedbackSystem.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminDashboardController : Controller
    {
        private readonly IAdminDashboardRepository _adminDashboardRepository;

        public AdminDashboardController(IAdminDashboardRepository adminDashboardRepository)
        {
            _adminDashboardRepository = adminDashboardRepository;
        }

        public IActionResult Index()
        {
            int? currentUserId = GetCurrentUserId();
            int? createdBy = User.IsInRole("SuperAdmin") ? null : currentUserId;
            var stats = _adminDashboardRepository.GetDashboardStats(createdBy);
            return View(stats);
        }

        public IActionResult Dashboard()
        {
            return RedirectToAction(nameof(Index));
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && int.TryParse(claim.Value, out int id))
            {
                return id;
            }
            return null;
        }
    }
}
