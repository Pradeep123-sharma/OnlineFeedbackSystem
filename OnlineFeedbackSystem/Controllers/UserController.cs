using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineFeedbackSystem.Models.Entities;
using OnlineFeedbackSystem.Models.ViewModels;
using OnlineFeedbackSystem.Repositories;
using OnlineFeedbackSystem.Services;

namespace OnlineFeedbackSystem.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly PasswordHasherService _passwordHasher;
        private readonly INotificationsRepository _notificationsRepository;

        public UserController(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IDepartmentRepository departmentRepository,
            PasswordHasherService passwordHasher,
            INotificationsRepository notificationsRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _departmentRepository = departmentRepository;
            _passwordHasher = passwordHasher;
            _notificationsRepository = notificationsRepository;
        }

        public IActionResult Index()
        {
            ViewData["ActiveNav"] = "Users";
            ViewData["PageTitle"] = "Users";
            ViewData["Title"] = "Users — Pulse";

            var users = _userRepository.GetAll();
            var roles = _roleRepository.GetAll();
            var departments = _departmentRepository.GetAll();

            var roleNameById = roles.ToDictionary(r => r.RoleId, r => r.RoleName);
            var departmentNameById = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

            var viewModels = users.Select(u => new UserListItemViewModel
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                RoleName = roleNameById.TryGetValue(u.RoleId, out var roleName) ? roleName : "Unknown",
                DepartmentName = u.DepartmentId.HasValue && departmentNameById.TryGetValue(u.DepartmentId.Value, out var deptName)
                    ? deptName
                    : "Unassigned",
                IsActive = u.IsActive
            }).ToList();

            return View(viewModels);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["ActiveNav"] = "Users";
            ViewData["PageTitle"] = "Add user";
            ViewData["Title"] = "Add User — Pulse";

            var model = new UserViewModel
            {
                AvailableRoles = GetRoleOptions(),
                AvailableDepartments = GetDepartmentOptions()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please check the form — some fields are missing or invalid.";
                return RedirectToAction(nameof(Index));
            }

            var existing = _userRepository.GetByEmail(model.Email);
            if (existing is not null)
            {
                TempData["Error"] = "A user with that email already exists.";
                return RedirectToAction(nameof(Index));
            }

            var user = new Users
            {
                FullName = model.FullName.Trim(),
                Email = model.Email.Trim(),
                RoleId = model.RoleId,
                DepartmentId = model.DepartmentId,
                IsActive = model.IsActive
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

            int newId = _userRepository.Create(user);

            if (newId <= 0)
            {
                TempData["Error"] = "Could not create the user.";
                return RedirectToAction(nameof(Index));
            }

            // Look up role name for notification
            var role = _roleRepository.GetById(model.RoleId);
            string roleName = role?.RoleName ?? "User";

            // Trigger SuperAdmin notification
            _notificationsRepository.NotifySuperAdmins(
                "System",
                $"User '{user.FullName}' ({user.Email}) was created successfully with role '{roleName}'."
            );

            TempData["Success"] = "User created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            _userRepository.Deactivate(id);
            TempData["Success"] = "User status updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        private List<RoleOptionViewModel> GetRoleOptions()
        {
            return _roleRepository.GetAll()
                .Select(r => new RoleOptionViewModel { RoleId = r.RoleId, RoleName = r.RoleName })
                .OrderBy(r => r.RoleName)
                .ToList();
        }

        private List<DepartmentOptionViewModel> GetDepartmentOptions()
        {
            return _departmentRepository.GetAll()
                .Where(d => d.IsActive)
                .Select(d => new DepartmentOptionViewModel { DepartmentId = d.DepartmentId, DepartmentName = d.DepartmentName })
                .OrderBy(d => d.DepartmentName)
                .ToList();
        }
    }
}
