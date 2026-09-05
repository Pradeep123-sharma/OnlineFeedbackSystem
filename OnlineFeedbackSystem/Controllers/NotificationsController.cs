using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineFeedbackSystem.Models.Entities;
using OnlineFeedbackSystem.Repositories;
using System.Security.Claims;

namespace OnlineFeedbackSystem.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class NotificationsController : Controller
    {
        private readonly INotificationsRepository _notificationsRepository;

        public NotificationsController(INotificationsRepository notificationsRepository)
        {
            _notificationsRepository = notificationsRepository;
        }

        private int? GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim != null && int.TryParse(idClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }

        [HttpGet]
        public IActionResult Index(string? filter)
        {
            int? userId = GetCurrentUserId();
            var model = _notificationsRepository.GetNotifications(userId, filter);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkAsRead(int id, string? returnFilter)
        {
            int? userId = GetCurrentUserId();
            _notificationsRepository.MarkAsRead(id, userId);
            TempData["SuccessMessage"] = "Notification marked as read.";
            return RedirectToAction(nameof(Index), new { filter = returnFilter });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkAllAsRead(string? returnFilter)
        {
            int? userId = GetCurrentUserId();
            _notificationsRepository.MarkAllAsRead(userId);
            TempData["SuccessMessage"] = "All notifications marked as read.";
            return RedirectToAction(nameof(Index), new { filter = returnFilter });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, string? returnFilter)
        {
            int? userId = GetCurrentUserId();
            _notificationsRepository.Delete(id, userId);
            TempData["SuccessMessage"] = "Notification removed.";
            return RedirectToAction(nameof(Index), new { filter = returnFilter });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearAllRead(string? returnFilter)
        {
            int? userId = GetCurrentUserId();
            _notificationsRepository.ClearAllRead(userId);
            TempData["SuccessMessage"] = "All read notifications have been cleared.";
            return RedirectToAction(nameof(Index), new { filter = returnFilter });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateTest(string type)
        {
            int? userId = GetCurrentUserId();

            // inside CreateTest action - replace existing notification factory with:
            var notification = type switch
            {
                "Submission" => new Notifications
                {
                    UserId = userId,
                    Type = "Submission",
                    Message = "A respondent submitted feedback for 'Student Course Feedback' with an overall rating of 5.0/5.0.",
                    IsRead = false
                },
                "LowRating" => new Notifications
                {
                    UserId = userId,
                    Type = "LowRating",
                    Message = "Low rating received: a student gave 1.0/5.0 on question 'Teaching Methodology'. Action recommended.",
                    IsRead = false
                },
                "Publication" => new Notifications
                {
                    UserId = userId,
                    Type = "Publication",
                    Message = "'Annual Faculty & Course Evaluation' is now live and accepting responses.",
                    IsRead = false
                },
                "Closure" => new Notifications
                {
                    UserId = userId,
                    Type = "Closure",
                    Message = "'Mid-Term Survey 2026' deadline has passed and the form is now closed.",
                    IsRead = false
                },
                _ => new Notifications
                {
                    UserId = userId,
                    Type = "System",
                    Message = "Database backup and index optimization completed successfully.",
                    IsRead = false
                }
            };

            _notificationsRepository.Create(notification);
            TempData["SuccessMessage"] = $"Test '{type}' notification generated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult GetUnreadCount()
        {
            int? userId = GetCurrentUserId();
            int unread = _notificationsRepository.GetUnreadCount(userId);
            return Json(new { count = unread });
        }
    }
}
