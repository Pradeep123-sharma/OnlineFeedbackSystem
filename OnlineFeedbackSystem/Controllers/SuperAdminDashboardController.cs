using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineFeedbackSystem.Repositories;

namespace OnlineFeedbackSystem.Controllers
{
    // [Authorize] alone means "any logged-in user." Adding Roles restricts
    // it further — only a cookie carrying ClaimTypes.Role = "SuperAdmin"
    // (set back in AccountController.Login) gets past this. Anyone else
    // hitting this URL is redirected to /Account/Login automatically —
    // that's options.LoginPath from the cookie config in Program.cs at work.
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminDashboardController : Controller
    {
        
         private readonly ISuperAdminDashboardRepository _dashboardRepository;

         public SuperAdminDashboardController(ISuperAdminDashboardRepository dashboardRepository)
         {
                _dashboardRepository = dashboardRepository;
         }

        public IActionResult Dashboard()
        {
            var stats = _dashboardRepository.GetStats();
            return View("~/Views/SuperAdmin/SuperAdminDashboard.cshtml", stats);
        }
    }
}
