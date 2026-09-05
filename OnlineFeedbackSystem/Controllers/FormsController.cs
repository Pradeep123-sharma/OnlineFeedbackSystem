using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineFeedbackSystem.Models.Entities;
using OnlineFeedbackSystem.Models.ViewModels;
using OnlineFeedbackSystem.Repositories;

namespace OnlineFeedbackSystem.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class FormsController : Controller
    {
        private readonly IFeedbackFormRepository _formRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly INotificationsRepository _notificationsRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IRespondentRepository _respondent_repository;
        private readonly IDepartmentRepository _departmentRepository;

        public FormsController(
            IFeedbackFormRepository formRepository,
            IQuestionRepository questionRepository,
            INotificationsRepository notificationsRepository,
            IAuditLogRepository auditLogRepository,
            IRespondentRepository respondentRepository,
            IDepartmentRepository departmentRepository)
        {
            _formRepository = formRepository;
            _questionRepository = questionRepository;
            _notificationsRepository = notificationsRepository;
            _auditLogRepository = auditLogRepository;
            _respondent_repository = respondentRepository;
            _departmentRepository = departmentRepository;
        }

        [HttpGet]
        public IActionResult Index(string? status, string? category, string? search, int? departmentId)
        {
            int? currentUserId = GetCurrentUserId();
            int? createdBy = User.IsInRole("SuperAdmin") ? null : currentUserId;

            var forms = _formRepository.GetAll(status, category, search, departmentId, createdBy);
            var categories = _formRepository.GetCategories();
            var activeDepartments = _departmentRepository.GetAll().Where(d => d.IsActive).OrderBy(d => d.DepartmentName).ToList();

            var viewModel = new FeedbackFormIndexViewModel
            {
                Forms = forms,
                SelectedStatus = status,
                SelectedCategory = category,
                SelectedDepartmentId = departmentId,
                SearchQuery = search,
                Categories = categories,
                Departments = activeDepartments
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var activeDepts = _departmentRepository.GetAll().Where(d => d.IsActive).OrderBy(d => d.DepartmentName).ToList();
            ViewBag.Categories = _formRepository.GetCategories();
            ViewBag.ActiveDepartments = activeDepts;

            var model = new FeedbackFormCreateEditViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(14),
                Status = "Draft",
                IsAnonymous = false,
                OneResponseOnly = true,
                AvailableDepartments = activeDepts
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(FeedbackFormCreateEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var activeDepts = _departmentRepository.GetAll().Where(d => d.IsActive).OrderBy(d => d.DepartmentName).ToList();
                ViewBag.Categories = _formRepository.GetCategories();
                ViewBag.ActiveDepartments = activeDepts;
                model.AvailableDepartments = activeDepts;
                return View(model);
            }

            int? currentUserId = GetCurrentUserId();

            var form = new FeedbackForms
            {
                Title = model.Title.Trim(),
                Description = model.Description?.Trim(),
                Category = model.Category.Trim(),
                DepartmentId = model.DepartmentId,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                IsAnonymous = model.IsAnonymous,
                OneResponseOnly = model.OneResponseOnly,
                Status = "Draft",
                CreatedBy = currentUserId ?? 1
            };

            try
            {
                int newId = _formRepository.Create(form);

                _auditLogRepository.InsertLog(
                    currentUserId,
                    "Create Form",
                    "FeedbackForms",
                    newId,
                    $"Created feedback form '{form.Title}' in category '{form.Category}'."
                );

                TempData["SuccessMessage"] = "Form created successfully! Now add questions from the Question Bank.";
                return RedirectToAction(nameof(Builder), new { id = newId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error creating form: " + ex.Message);
                var activeDepts = _departmentRepository.GetAll().Where(d => d.IsActive).OrderBy(d => d.DepartmentName).ToList();
                ViewBag.Categories = _formRepository.GetCategories();
                ViewBag.ActiveDepartments = activeDepts;
                model.AvailableDepartments = activeDepts;
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var form = _formRepository.GetById(id);
            if (!HasFormAccess(form, out var failureResult))
            {
                return failureResult!;
            }

            var activeDepts = _departmentRepository.GetAll().Where(d => d.IsActive || d.DepartmentId == form!.DepartmentId).OrderBy(d => d.DepartmentName).ToList();
            ViewBag.Categories = _formRepository.GetCategories();
            ViewBag.ActiveDepartments = activeDepts;

            var model = new FeedbackFormCreateEditViewModel
            {
                Id = form!.Id,
                Title = form.Title,
                Description = form.Description,
                Category = form.Category,
                DepartmentId = form.DepartmentId,
                DepartmentName = form.DepartmentName,
                StartDate = form.StartDate,
                EndDate = form.EndDate,
                IsAnonymous = form.IsAnonymous,
                OneResponseOnly = form.OneResponseOnly,
                Status = form.Status,
                AvailableDepartments = activeDepts
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, FeedbackFormCreateEditViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var existingForm = _formRepository.GetById(id);
            if (!HasFormAccess(existingForm, out var failureResult))
            {
                return failureResult!;
            }

            if (!ModelState.IsValid)
            {
                var activeDepts = _departmentRepository.GetAll().Where(d => d.IsActive || d.DepartmentId == model.DepartmentId).OrderBy(d => d.DepartmentName).ToList();
                ViewBag.Categories = _formRepository.GetCategories();
                ViewBag.ActiveDepartments = activeDepts;
                model.AvailableDepartments = activeDepts;
                return View(model);
            }

            var form = new FeedbackForms
            {
                Id = model.Id,
                Title = model.Title.Trim(),
                Description = model.Description?.Trim(),
                Category = model.Category.Trim(),
                DepartmentId = model.DepartmentId,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                IsAnonymous = model.IsAnonymous,
                OneResponseOnly = model.OneResponseOnly,
                Status = model.Status,
                CreatedBy = existingForm!.CreatedBy
            };

            try
            {
                _formRepository.Update(form);
                int? currentUserId = GetCurrentUserId();

                _auditLogRepository.InsertLog(
                    currentUserId,
                    "Update Form",
                    "FeedbackForms",
                    form.Id,
                    $"Updated form settings for '{form.Title}'."
                );

                TempData["SuccessMessage"] = "Feedback form settings updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error updating form: " + ex.Message);
                var activeDepts = _departmentRepository.GetAll().Where(d => d.IsActive || d.DepartmentId == model.DepartmentId).OrderBy(d => d.DepartmentName).ToList();
                ViewBag.Categories = _formRepository.GetCategories();
                ViewBag.ActiveDepartments = activeDepts;
                model.AvailableDepartments = activeDepts;
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Builder(int id, string? bankSearch = null, string? bankCategory = null)
        {
            var form = _formRepository.GetById(id);
            if (!HasFormAccess(form, out var failureResult))
            {
                return failureResult!;
            }

            var assignedQuestions = _formRepository.GetFormQuestions(id);
            var assignedIds = assignedQuestions.Select(q => q.QuestionId).ToHashSet();

            var allBankQuestions = _questionRepository.GetAll(bankCategory, null, bankSearch);
            var availableBankQuestions = allBankQuestions
                .Where(q => !assignedIds.Contains(q.Id) && q.IsActive)
                .Select(q => new QuestionListItemViewModel
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    QuestionType = q.QuestionType,
                    Category = q.Category,
                    IsActive = q.IsActive,
                    Options = q.Options?.Select(o => o.OptionText).ToList() ?? new List<string>()
                })
                .ToList();

            var viewModel = new FormBuilderViewModel
            {
                FormId = form!.Id,
                Title = form.Title,
                Description = form.Description,
                Category = form.Category,
                DepartmentId = form.DepartmentId,
                DepartmentName = form.DepartmentName,
                Status = form.Status,
                AssignedQuestions = assignedQuestions,
                AvailableBankQuestions = availableBankQuestions,
                BankSearchQuery = bankSearch,
                BankSelectedCategory = bankCategory,
                Categories = _questionRepository.GetCategories()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddQuestion(int formId, int questionId, bool isRequired = true)
        {
            var form = _formRepository.GetById(formId);
            if (!HasFormAccess(form, out var failureResult))
            {
                return failureResult!;
            }

            try
            {
                _formRepository.AddFormQuestion(formId, questionId, isRequired);
                int? currentUserId = GetCurrentUserId();

                _auditLogRepository.InsertLog(
                    currentUserId,
                    "Add Question To Form",
                    "FormQuestions",
                    formId,
                    $"Attached question #{questionId} to form #{formId} (Required={isRequired})."
                );

                TempData["SuccessMessage"] = "Question added to form!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Could not add question: " + ex.Message;
            }

            return RedirectToAction(nameof(Builder), new { id = formId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveQuestion(int formId, int questionId)
        {
            var form = _formRepository.GetById(formId);
            if (!HasFormAccess(form, out var failureResult))
            {
                return failureResult!;
            }

            try
            {
                _formRepository.RemoveFormQuestion(formId, questionId);
                int? currentUserId = GetCurrentUserId();

                _auditLogRepository.InsertLog(
                    currentUserId,
                    "Remove Question From Form",
                    "FormQuestions",
                    formId,
                    $"Removed question #{questionId} from form #{formId}."
                );

                TempData["SuccessMessage"] = "Question removed from form.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Could not remove question: " + ex.Message;
            }

            return RedirectToAction(nameof(Builder), new { id = formId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleQuestionRequired(int formId, int questionId, bool isRequired)
        {
            var form = _formRepository.GetById(formId);
            if (!HasFormAccess(form, out var failureResult))
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return Json(new { success = false, message = "Access denied." });
                }
                return failureResult!;
            }

            try
            {
                bool success = _formRepository.UpdateFormQuestionRequired(formId, questionId, isRequired);
                int? currentUserId = GetCurrentUserId();

                _auditLogRepository.InsertLog(
                    currentUserId,
                    "Toggle Question Required",
                    "FormQuestions",
                    formId,
                    $"Set question #{questionId} in form #{formId} to {(isRequired ? "Required" : "Optional")}."
                );

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return Json(new { success = true, isRequired = isRequired, message = $"Question is now {(isRequired ? "Required" : "Optional")}." });
                }

                TempData["SuccessMessage"] = $"Question set to {(isRequired ? "Required" : "Optional")}.";
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return Json(new { success = false, message = ex.Message });
                }

                TempData["ErrorMessage"] = "Could not update required status: " + ex.Message;
            }

            return RedirectToAction(nameof(Builder), new { id = formId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MoveQuestion(int formId, int questionId, string direction)
        {
            var form = _formRepository.GetById(formId);
            if (!HasFormAccess(form, out var failureResult))
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return Json(new { success = false, message = "Access denied." });
                }
                return failureResult!;
            }

            try
            {
                bool success = _formRepository.MoveQuestion(formId, questionId, direction);
                int? currentUserId = GetCurrentUserId();

                if (success)
                {
                    _auditLogRepository.InsertLog(
                        currentUserId,
                        "Reorder Question",
                        "FormQuestions",
                        formId,
                        $"Moved question #{questionId} {direction.ToLower()} in form #{formId}."
                    );
                }

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return Json(new { success = success, message = success ? "Order updated." : "Could not move question." });
                }

                if (success)
                {
                    TempData["SuccessMessage"] = $"Question order moved {direction.ToLower()}.";
                }
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return Json(new { success = false, message = ex.Message });
                }

                TempData["ErrorMessage"] = "Could not move question: " + ex.Message;
            }

            return RedirectToAction(nameof(Builder), new { id = formId });
        }

        [HttpPost]
        public IActionResult ReorderQuestions([FromBody] ReorderQuestionsRequestDto request)
        {
            if (request == null || request.FormId <= 0 || request.OrderedQuestionIds == null || request.OrderedQuestionIds.Count == 0)
            {
                return Json(new { success = false, message = "Invalid reorder payload." });
            }

            var form = _formRepository.GetById(request.FormId);
            if (!HasFormAccess(form, out _))
            {
                return Json(new { success = false, message = "Access denied." });
            }

            try
            {
                bool success = _formRepository.ReorderAllQuestions(request.FormId, request.OrderedQuestionIds);
                int? currentUserId = GetCurrentUserId();

                _auditLogRepository.InsertLog(
                    currentUserId,
                    "Reorder All Questions",
                    "FormQuestions",
                    request.FormId,
                    $"Updated custom question order in form #{request.FormId} ({request.OrderedQuestionIds.Count} questions)."
                );

                return Json(new { success = success, message = "Questions reordered successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Preview(int id)
        {
            var form = _formRepository.GetById(id);
            if (!HasFormAccess(form, out var failureResult))
            {
                return failureResult!;
            }

            // Get the respondent-facing form representation
            var respondentForm = _respondent_repository.GetFormForFilling(id);
            if (respondentForm == null)
            {
                TempData["ErrorMessage"] = "Feedback form not found.";
                return RedirectToAction(nameof(Index));
            }

            // Map RespondentFormFillingViewModel -> FormBuilderViewModel (shape expected by Preview.cshtml)
            var assignedQuestions = respondentForm.Questions
                .OrderBy(q => q.DisplayOrder)
                .Select(q => new FormQuestionItemViewModel
                {
                    FormQuestionId = 0,
                    FormId = respondentForm.FormId,
                    QuestionId = q.QuestionId,
                    QuestionText = q.QuestionText,
                    QuestionType = q.QuestionType,
                    Category = q.Category,
                    IsRequired = q.IsRequired,
                    DisplayOrder = q.DisplayOrder,
                    Options = q.Options ?? new List<string>()
                })
                .ToList();

            var previewModel = new FormBuilderViewModel
            {
                FormId = respondentForm.FormId,
                Title = respondentForm.Title,
                Description = respondentForm.Description,
                Category = respondentForm.Category,
                DepartmentId = respondentForm.DepartmentId,
                DepartmentName = respondentForm.DepartmentName,
                Status = "Preview",
                AssignedQuestions = assignedQuestions,
                AvailableBankQuestions = new List<QuestionListItemViewModel>(),
                BankSearchQuery = null,
                BankSelectedCategory = null,
                Categories = _questionRepository.GetCategories()
            };

            ViewBag.IsPreview = true;
            return View("~/Views/Forms/Preview.cshtml", previewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Publish(int id)
        {
            var form = _formRepository.GetById(id);
            if (!HasFormAccess(form, out var failureResult))
            {
                return failureResult!;
            }

            try
            {
                var questions = _formRepository.GetFormQuestions(id);
                if (!questions.Any())
                {
                    TempData["ErrorMessage"] = "Cannot publish form without any questions. Please add questions first.";
                    return RedirectToAction(nameof(Builder), new { id });
                }

                _formRepository.ChangeStatus(id, "Published");
                int? currentUserId = GetCurrentUserId();

                _auditLogRepository.InsertLog(
                    currentUserId,
                    "Publish Form",
                    "FeedbackForms",
                    id,
                    $"Published feedback form '{form!.Title}'."
                );

                _notificationsRepository.NotifySuperAdmins(
                    "Publication",
                    $"Form '{form.Title}' has been published and is now accepting responses.",
                    id
                );

                TempData["SuccessMessage"] = "Form published successfully! Respondents can now access and submit responses.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Could not publish form: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Close(int id)
        {
            var form = _formRepository.GetById(id);
            if (!HasFormAccess(form, out var failureResult))
            {
                return failureResult!;
            }

            try
            {
                _formRepository.ChangeStatus(id, "Closed");
                int? currentUserId = GetCurrentUserId();

                var display = form?.Title ?? ("#" + id);
                _auditLogRepository.InsertLog(
                    currentUserId,
                    "Close Form",
                    "FeedbackForms",
                    id,
                    $"Closed feedback form '{display}'."
                );

                TempData["SuccessMessage"] = "Feedback form closed. No further responses will be accepted.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Could not close form: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var form = _formRepository.GetById(id);
            if (!HasFormAccess(form, out var failureResult))
            {
                return failureResult!;
            }

            try
            {
                _formRepository.Delete(id);
                int? currentUserId = GetCurrentUserId();

                var display = form?.Title ?? ("#" + id);
                _auditLogRepository.InsertLog(
                    currentUserId,
                    "Delete Form",
                    "FeedbackForms",
                    id,
                    $"Deleted feedback form '{display}'."
                );

                TempData["SuccessMessage"] = "Feedback form deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Could not delete form: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool HasFormAccess(FeedbackForms? form, out IActionResult? failureResult)
        {
            if (form == null)
            {
                TempData["ErrorMessage"] = "Feedback form not found.";
                failureResult = RedirectToAction(nameof(Index));
                return false;
            }

            if (!User.IsInRole("SuperAdmin"))
            {
                int? currentUserId = GetCurrentUserId();
                if (form.CreatedBy != currentUserId)
                {
                    TempData["ErrorMessage"] = "You do not have permission to view or modify this form.";
                    failureResult = RedirectToAction(nameof(Index));
                    return false;
                }
            }

            failureResult = null;
            return true;
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