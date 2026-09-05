using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineFeedbackSystem.Models.ViewModels;
using OnlineFeedbackSystem.Repositories;

namespace OnlineFeedbackSystem.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin,DepartmentManager")]
    public class ResponsesController : Controller
    {
        private readonly IResponseReviewRepository _responseRepository;
        private readonly IFeedbackFormRepository _formRepository;

        public ResponsesController(
            IResponseReviewRepository responseRepository,
            IFeedbackFormRepository formRepository)
        {
            _responseRepository = responseRepository;
            _formRepository = formRepository;
        }

        [HttpGet]
        public IActionResult Index(int? formId)
        {
            int? currentUserId = GetCurrentUserId();
            int? createdBy = User.IsInRole("SuperAdmin") ? null : currentUserId;

            var forms = _formRepository.GetAll(createdBy: createdBy);
            var allowedFormIds = forms.Select(f => f.Id).ToHashSet();

            if (formId.HasValue && !User.IsInRole("SuperAdmin"))
            {
                if (!allowedFormIds.Contains(formId.Value))
                {
                    TempData["ErrorMessage"] = "You do not have permission to view responses for this form.";
                    return RedirectToAction(nameof(Index), new { formId = (int?)null });
                }
            }

            var allResponses = _responseRepository.GetResponses(formId);
            var responses = User.IsInRole("SuperAdmin")
                ? allResponses
                : allResponses.Where(r => allowedFormIds.Contains(r.FormId)).ToList();

            var viewModel = new ResponseListViewModel
            {
                SelectedFormId = formId,
                AvailableForms = forms,
                Responses = responses
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Detail(int id)
        {
            var detail = _responseRepository.GetResponseDetail(id);
            if (detail == null)
            {
                TempData["ErrorMessage"] = "Response submission not found.";
                return RedirectToAction(nameof(Index));
            }

            if (!User.IsInRole("SuperAdmin"))
            {
                var form = _formRepository.GetById(detail.FormId);
                if (form == null || form.CreatedBy != GetCurrentUserId())
                {
                    TempData["ErrorMessage"] = "You do not have permission to view this response.";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(detail);
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
