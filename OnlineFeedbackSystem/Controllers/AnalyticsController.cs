using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineFeedbackSystem.Models.ViewModels;
using OnlineFeedbackSystem.Repositories;

namespace OnlineFeedbackSystem.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin,DepartmentManager,ReportViewer")]
    public class AnalyticsController : Controller
    {
        private readonly IAnalyticsRepository _analyticsRepository;
        private readonly IFeedbackFormRepository _formRepository;
        private readonly IResponseReviewRepository _responseReviewRepository;

        public AnalyticsController(
            IAnalyticsRepository analyticsRepository,
            IFeedbackFormRepository formRepository,
            IResponseReviewRepository responseReviewRepository)
        {
            _analyticsRepository = analyticsRepository;
            _formRepository = formRepository;
            _responseReviewRepository = responseReviewRepository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            int? currentUserId = GetCurrentUserId();
            int? createdBy = User.IsInRole("SuperAdmin") ? null : currentUserId;

            var forms = _formRepository.GetAll(createdBy: createdBy);
            var viewModel = new AnalyticsDashboardIndexViewModel
            {
                Forms = forms
            };
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Form(int id)
        {
            var form = _formRepository.GetById(id);
            if (form == null)
            {
                TempData["ErrorMessage"] = "Feedback form not found.";
                return RedirectToAction(nameof(Index));
            }

            if (!User.IsInRole("SuperAdmin"))
            {
                int? currentUserId = GetCurrentUserId();
                if (form.CreatedBy != currentUserId)
                {
                    TempData["ErrorMessage"] = "You do not have permission to view analytics for this form.";
                    return RedirectToAction(nameof(Index));
                }
            }

            var analytics = _analyticsRepository.GetFormAnalytics(id);
            if (analytics == null)
            {
                TempData["ErrorMessage"] = "Form analytics not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(analytics);
        }

        [HttpGet]
        public IActionResult Export(int id)
        {
            var form = _formRepository.GetById(id);
            if (form == null) return NotFound();

            if (!User.IsInRole("SuperAdmin"))
            {
                int? currentUserId = GetCurrentUserId();
                if (form.CreatedBy != currentUserId)
                {
                    TempData["ErrorMessage"] = "You do not have permission to export responses for this form.";
                    return RedirectToAction(nameof(Index));
                }
            }

            var responses = _responseReviewRepository.GetResponses(id);

            var sb = new StringBuilder();
            sb.AppendLine("ResponseId,SubmissionToken,SubmittedAt,RespondentName,AverageRating,Status");

            foreach (var r in responses)
            {
                string name = form.IsAnonymous ? "Anonymous" : r.RespondentName.Replace(",", " ");
                string rating = r.AverageRating.HasValue ? r.AverageRating.Value.ToString("0.0") : "N/A";
                sb.AppendLine($"{r.Id},{r.SubmissionToken},{r.SubmittedAt:yyyy-MM-dd HH:mm},{name},{rating},Submitted");
            }

            byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString());
            string fileName = $"Feedback_Report_{form.Title.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.csv";

            return File(buffer, "text/csv", fileName);
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
