using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineFeedbackSystem.Models.ViewModels;
using OnlineFeedbackSystem.Repositories;

namespace OnlineFeedbackSystem.Controllers
{
    public class RespondentController : Controller
    {
        private readonly IRespondentRepository _respondentRepository;
        private readonly IFeedbackFormRepository _formRepository;
        private readonly INotificationsRepository _notificationsRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public RespondentController(
            IRespondentRepository respondentRepository,
            IFeedbackFormRepository formRepository,
            INotificationsRepository notificationsRepository,
            IAuditLogRepository auditLogRepository)
        {
            _respondentRepository = respondentRepository;
            _formRepository = formRepository;
            _notificationsRepository = notificationsRepository;
            _auditLogRepository = auditLogRepository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            int? userId = GetCurrentUserId();
            string? userName = User.Identity?.IsAuthenticated == true ? User.Identity.Name : null;
            var dashboardModel = _respondentRepository.GetRespondentDashboard(userId, userName);
            return View(dashboardModel);
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Fill(int id)
        {
            int? userId = GetCurrentUserId();

            var form = _respondentRepository.GetFormForFilling(id);
            if (form == null)
            {
                TempData["ErrorMessage"] = "Feedback form is not available or has expired.";
                return RedirectToAction(nameof(Index));
            }

            if (userId.HasValue && form.OneResponseOnly)
            {
                bool alreadySubmitted = _respondentRepository.HasUserSubmitted(id, userId.Value);
                if (alreadySubmitted)
                {
                    TempData["ErrorMessage"] = "You have already submitted your response for this feedback form.";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(form);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Submit(RespondentFormFillingViewModel model)
        {
            int? userId = GetCurrentUserId();

            // Validate required questions
            if (model.Questions != null)
            {
                foreach (var q in model.Questions)
                {
                    if (q.IsRequired)
                    {
                        bool isAnswered = false;

                        // 1) Numeric answers (covers 5-star and 1-10 rating controls)
                        if (q.NumericAnswer.HasValue && q.NumericAnswer.Value > 0)
                        {
                            isAnswered = true;
                        }
                        // 2) Multiple choice (checkboxes)
                        else if (q.SelectedChoices != null && q.SelectedChoices.Count > 0)
                        {
                            isAnswered = true;
                        }
                        // 3) Textual answers, single choice dropdown, yes/no stored in TextAnswer, etc.
                        else if (!string.IsNullOrWhiteSpace(q.TextAnswer))
                        {
                            isAnswered = true;
                        }

                        if (!isAnswered)
                        {
                            ModelState.AddModelError("", $"Please answer the required question: {q.QuestionText}");
                        }
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                return View("Fill", model);
            }

            try
            {
                string token = _respondentRepository.SubmitFeedback(model.FormId, userId, model.Questions ?? new List<RespondentQuestionInputViewModel>());

                // Retrieve form info for notifications
                var formEntity = _formRepository.GetById(model.FormId);
                string formTitle = formEntity?.Title ?? (!string.IsNullOrWhiteSpace(model.Title) ? model.Title : "Feedback Form");

                string respondentName = (User.Identity?.IsAuthenticated == true && !string.IsNullOrWhiteSpace(User.Identity.Name))
                    ? User.Identity.Name
                    : "Anonymous respondent";

                // Log audit entry
                _auditLogRepository.InsertLog(
                    userId,
                    "Submit Feedback",
                    "FeedbackResponses",
                    model.FormId,
                    $"Submitted response for form '{formTitle}' (Token: {token})."
                );

                // 1. Notify Creator Admin
                if (formEntity != null && formEntity.CreatedBy > 0)
                {
                    _notificationsRepository.NotifyUser(
                        formEntity.CreatedBy,
                        "Submission",
                        $"{respondentName} submitted a response for '{formTitle}'.",
                        model.FormId
                    );
                }

                // 2. Notify SuperAdmins
                _notificationsRepository.NotifySuperAdmins(
                    "Submission",
                    $"New response received for '{formTitle}' from {respondentName}.",
                    model.FormId
                );

                return RedirectToAction(nameof(Confirmation), new { token = token, title = formTitle });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred submitting your feedback: " + ex.Message);
                return View("Fill", model);
            }
        }

        [HttpGet]
        public IActionResult Confirmation(string token, string? title)
        {
            var model = new SubmissionConfirmationViewModel
            {
                SubmissionToken = token,
                FormTitle = string.IsNullOrEmpty(title) ? "Feedback Survey" : title,
                SubmittedAt = DateTime.Now
            };

            return View(model);
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
