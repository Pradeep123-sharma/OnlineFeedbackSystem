using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using OnlineFeedbackSystem.Models.AccountModels;
using OnlineFeedbackSystem.Models.Entities;
using OnlineFeedbackSystem.Repositories;
using OnlineFeedbackSystem.Services;

namespace OnlineFeedbackSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly PasswordHasherService _passwordHasher;
        private readonly INotificationsRepository _notificationsRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public AccountController(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            PasswordHasherService passwordHasher,
            INotificationsRepository notificationsRepository,
            IAuditLogRepository auditLogRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
            _notificationsRepository = notificationsRepository;
            _auditLogRepository = auditLogRepository;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("SuperAdmin"))
                    return RedirectToAction("Dashboard", "SuperAdminDashboard");
                if (User.IsInRole("Admin"))
                    return RedirectToAction("Index", "AdminDashboard");
                return RedirectToAction("Index", "Respondent");
            }

            return View("~/Views/Home/Login.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Home/Login.cshtml", model);
            }

            var user = _userRepository.GetByEmail(model.Email);

            if (user is null || !user.IsActive || !_passwordHasher.VerifyPassword(user, model.Password))
            {
                _auditLogRepository.InsertLog(
                    null,
                    "Failed Login Attempt",
                    "Users",
                    null,
                    $"Failed login attempt with email: {model.Email}"
                );

                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View("~/Views/Home/Login.cshtml", model);
            }

            var role = _roleRepository.GetById(user.RoleId);
            var roleName = role?.RoleName ?? "Respondent";

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, roleName)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = false });

            _auditLogRepository.InsertLog(
                user.UserId,
                "User Login",
                "Users",
                user.UserId,
                $"User '{user.FullName}' ({user.Email}) logged in successfully with role [{roleName}]."
            );

            if (roleName.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Dashboard", "SuperAdminDashboard");
            }
            else if (roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "AdminDashboard");
            }
            else
            {
                return RedirectToAction("Index", "Respondent");
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Respondent");
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existing = _userRepository.GetByEmail(model.Email);
            if (existing != null)
            {
                ModelState.AddModelError("Email", "An account with this email address already exists.");
                return View(model);
            }

            // Find Respondent role
            var roles = _roleRepository.GetAll();
            var respondentRole = roles.FirstOrDefault(r => r.RoleName.Equals("Respondent", StringComparison.OrdinalIgnoreCase))
                                 ?? roles.FirstOrDefault(r => !r.RoleName.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) && !r.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                                 ?? roles.FirstOrDefault();

            int roleId = respondentRole?.RoleId ?? 3; // Default fallback to 3 (Respondent)

            var user = new Users
            {
                FullName = model.FullName.Trim(),
                Email = model.Email.Trim(),
                RoleId = roleId,
                DepartmentId = model.DepartmentId,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

            int newId = _userRepository.Create(user);
            if (newId <= 0)
            {
                ModelState.AddModelError(string.Empty, "Could not complete registration. Please try again.");
                return View(model);
            }

            _auditLogRepository.InsertLog(
                newId,
                "User Registration",
                "Users",
                newId,
                $"New respondent registered: '{user.FullName}' ({user.Email})."
            );

            // Notify SuperAdmin about new respondent registration
            _notificationsRepository.NotifySuperAdmins(
                "System",
                $"New respondent registration: {user.FullName} ({user.Email}) created an account for respondent purpose."
            );

            TempData["Success"] = "Account registered successfully! Please log in to browse and fill feedback surveys.";
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            int? currentUserId = null;
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim != null && int.TryParse(idClaim.Value, out int parsedId))
            {
                currentUserId = parsedId;
            }

            _auditLogRepository.InsertLog(
                currentUserId,
                "User Logout",
                "Users",
                currentUserId,
                $"User '{User.Identity?.Name ?? "User"}' logged out."
            );

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
