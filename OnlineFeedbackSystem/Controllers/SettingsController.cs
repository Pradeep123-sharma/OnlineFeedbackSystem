using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineFeedbackSystem.Models.ViewModels;
using OnlineFeedbackSystem.Repositories;
using System.Security.Claims;

namespace OnlineFeedbackSystem.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class SettingsController : Controller
    {
        private readonly ISettingsRepository _settingsRepository;

        public SettingsController(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        private int GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim != null && int.TryParse(idClaim.Value, out int userId))
            {
                return userId;
            }
            return 1;
        }

        [HttpGet]
        public IActionResult Index(string? tab)
        {
            int userId = GetCurrentUserId();
            var model = _settingsRepository.GetSettings(userId);
            if (!string.IsNullOrWhiteSpace(tab))
            {
                model.ActiveTab = tab;
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(AdminSettingsViewModel model)
        {
            int userId = GetCurrentUserId();
            model.ActiveTab = "profile";

            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                ModelState.AddModelError(nameof(model.FullName), "Full name is required.");
            }
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "Email address is required.");
            }

            if (!ModelState.IsValid)
            {
                var refreshed = _settingsRepository.GetSettings(userId);
                refreshed.FullName = model.FullName;
                refreshed.Email = model.Email;
                refreshed.ActiveTab = "profile";
                return View("Index", refreshed);
            }

            try
            {
                _settingsRepository.UpdateProfile(userId, model.FullName, model.Email);
                TempData["SuccessMessage"] = "Profile details updated successfully.";
                return RedirectToAction(nameof(Index), new { tab = "profile" });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index), new { tab = "profile" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(AdminSettingsViewModel model)
        {
            int userId = GetCurrentUserId();
            model.ActiveTab = "profile";

            if (string.IsNullOrWhiteSpace(model.CurrentPassword))
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "Current password is required.");
            }
            if (string.IsNullOrWhiteSpace(model.NewPassword) || model.NewPassword.Length < 6)
            {
                ModelState.AddModelError(nameof(model.NewPassword), "New password must be at least 6 characters long.");
            }
            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError(nameof(model.ConfirmPassword), "New password and confirmation do not match.");
            }

            if (!ModelState.IsValid)
            {
                var refreshed = _settingsRepository.GetSettings(userId);
                refreshed.ActiveTab = "profile";
                return View("Index", refreshed);
            }

            var result = _settingsRepository.ChangePassword(userId, model.CurrentPassword!, model.NewPassword!);
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Index), new { tab = "profile" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateNotificationPreferences(AdminSettingsViewModel model)
        {
            int userId = GetCurrentUserId();
            var existing = _settingsRepository.GetSettings(userId);

            existing.EmailOnNewResponse = model.EmailOnNewResponse;
            existing.LowRatingAlertThreshold = model.LowRatingAlertThreshold;
            existing.DailyDigestEnabled = model.DailyDigestEnabled;
            existing.NotifyOnFormClosure = model.NotifyOnFormClosure;
            existing.NotifyOnMilestone = model.NotifyOnMilestone;

            _settingsRepository.SaveSettings(userId, existing);
            TempData["SuccessMessage"] = "Notification preferences and alert rules saved.";
            return RedirectToAction(nameof(Index), new { tab = "notifications" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateFormDefaults(AdminSettingsViewModel model)
        {
            int userId = GetCurrentUserId();
            var existing = _settingsRepository.GetSettings(userId);

            existing.DefaultValidityDays = model.DefaultValidityDays;
            existing.DefaultIsAnonymous = model.DefaultIsAnonymous;
            existing.DefaultOneResponseOnly = model.DefaultOneResponseOnly;
            existing.DefaultWelcomeMessage = model.DefaultWelcomeMessage?.Trim() ?? string.Empty;
            existing.DefaultThankYouMessage = model.DefaultThankYouMessage?.Trim() ?? string.Empty;

            _settingsRepository.SaveSettings(userId, existing);
            TempData["SuccessMessage"] = "Feedback form system defaults updated.";
            return RedirectToAction(nameof(Index), new { tab = "formDefaults" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateSystemPreferences(AdminSettingsViewModel model)
        {
            int userId = GetCurrentUserId();
            var existing = _settingsRepository.GetSettings(userId);

            existing.MaxQuestionsPerForm = model.MaxQuestionsPerForm;
            existing.ExportFormatPreference = model.ExportFormatPreference ?? "Excel";
            existing.SessionTimeoutMinutes = model.SessionTimeoutMinutes;

            _settingsRepository.SaveSettings(userId, existing);
            TempData["SuccessMessage"] = "System and export preferences saved.";
            return RedirectToAction(nameof(Index), new { tab = "system" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetDefaults()
        {
            int userId = GetCurrentUserId();
            _settingsRepository.ResetToDefaults(userId);
            TempData["SuccessMessage"] = "Preferences restored to system default values.";
            return RedirectToAction(nameof(Index));
        }
    }
}
