using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineFeedbackSystem.Models.ViewModels;
using OnlineFeedbackSystem.Repositories;

namespace OnlineFeedbackSystem.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class RoleController : Controller
    {
        private readonly IRoleRepository _roleRepository;

        // Responsibility text from your doc's Section 3 table, matched by
        // role name with spaces/slashes stripped and case ignored — so it
        // matches whatever exact casing your seed data used ("SuperAdmin",
        // "Super Admin", etc.) without needing to know that in advance.
        // Anything that doesn't match falls back to "—" rather than
        // guessing, since a wrong description here would be actively
        // misleading on a page whose whole point is being informational.
        private static readonly Dictionary<string, string> ResponsibilityByNormalizedName = new()
        {
            ["superadmin"] = "Complete system configuration and access",
            ["admin"] = "Forms, questions, publication, responses and reports",
            ["departmentmanager"] = "View assigned department feedback",
            ["respondent"] = "Fill and submit feedback",
            ["reportviewer"] = "Read-only dashboard/report access"
        };

        public RoleController(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public IActionResult Index()
        {
            ViewData["ActiveNav"] = "Roles";
            ViewData["PageTitle"] = "Roles & Permissions";
            ViewData["Title"] = "Roles — Pulse";

            var roles = _roleRepository.GetAll()
                .Select(r => new RoleListItemViewModel
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName,
                    Responsibility = ResponsibilityFor(r.RoleName)
                })
                .OrderBy(r => r.RoleId)
                .ToList();

            return View(roles);
        }

        private static string ResponsibilityFor(string roleName)
        {
            var normalized = roleName.Replace(" ", "").Replace("/", "").ToLowerInvariant();
            return ResponsibilityByNormalizedName.TryGetValue(normalized, out var text)
                ? text
                : "—";
        }
    }
}