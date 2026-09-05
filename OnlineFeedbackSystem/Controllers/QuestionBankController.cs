using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineFeedbackSystem.Models.Entities;
using OnlineFeedbackSystem.Models.ViewModels;
using OnlineFeedbackSystem.Repositories;

namespace OnlineFeedbackSystem.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class QuestionBankController : Controller
    {
        private readonly IQuestionRepository _questionRepository;

        public QuestionBankController(IQuestionRepository questionRepository)
        {
            _questionRepository = questionRepository;
        }

        [HttpGet]
        public IActionResult Index(string? category, string? type, string? search)
        {
            var questions = _questionRepository.GetAll(category, type, search);
            var categories = _questionRepository.GetCategories();

            var viewModel = new QuestionBankIndexViewModel
            {
                SelectedCategory = category,
                SelectedType = type,
                SearchQuery = search,
                Categories = categories,
                QuestionTypes = GetAvailableQuestionTypes(),
                Questions = questions.Select(q => new QuestionListItemViewModel
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    QuestionType = q.QuestionType,
                    Category = q.Category,
                    IsActive = q.IsActive,
                    Options = q.Options.Select(o => o.OptionText).ToList()
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = _questionRepository.GetCategories();
            ViewBag.QuestionTypes = GetAvailableQuestionTypes();

            var model = new QuestionCreateEditViewModel
            {
                IsActive = true,
                QuestionType = "Rating5",
                Options = new List<string>()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(QuestionCreateEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _questionRepository.GetCategories();
                ViewBag.QuestionTypes = GetAvailableQuestionTypes();
                return View(model);
            }

            var question = new Questions
            {
                QuestionText = model.QuestionText.Trim(),
                QuestionType = model.QuestionType,
                Category = model.Category.Trim(),
                IsActive = model.IsActive
            };

            var options = (model.Options ?? new List<string>())
                .Where(o => !string.IsNullOrWhiteSpace(o))
                .Select((o, idx) => new QuestionOptions
                {
                    OptionText = o.Trim(),
                    OptionValue = o.Trim(),
                    DisplayOrder = idx + 1
                }).ToList();

            try
            {
                int id = _questionRepository.Create(question, options);
                TempData["SuccessMessage"] = "Question added to Question Bank successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error creating question: " + ex.Message);
                ViewBag.Categories = _questionRepository.GetCategories();
                ViewBag.QuestionTypes = GetAvailableQuestionTypes();
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var question = _questionRepository.GetById(id);
            if (question == null)
            {
                TempData["ErrorMessage"] = "Question not found.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = _questionRepository.GetCategories();
            ViewBag.QuestionTypes = GetAvailableQuestionTypes();

            var model = new QuestionCreateEditViewModel
            {
                Id = question.Id,
                QuestionText = question.QuestionText,
                QuestionType = question.QuestionType,
                Category = question.Category,
                IsActive = question.IsActive,
                Options = question.Options.Select(o => o.OptionText).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, QuestionCreateEditViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _questionRepository.GetCategories();
                ViewBag.QuestionTypes = GetAvailableQuestionTypes();
                return View(model);
            }

            var question = new Questions
            {
                Id = model.Id,
                QuestionText = model.QuestionText.Trim(),
                QuestionType = model.QuestionType,
                Category = model.Category.Trim(),
                IsActive = model.IsActive
            };

            var options = (model.Options ?? new List<string>())
                .Where(o => !string.IsNullOrWhiteSpace(o))
                .Select((o, idx) => new QuestionOptions
                {
                    QuestionId = model.Id,
                    OptionText = o.Trim(),
                    OptionValue = o.Trim(),
                    DisplayOrder = idx + 1
                }).ToList();

            try
            {
                _questionRepository.Update(question, options);
                TempData["SuccessMessage"] = "Question updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error updating question: " + ex.Message);
                ViewBag.Categories = _questionRepository.GetCategories();
                ViewBag.QuestionTypes = GetAvailableQuestionTypes();
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            _questionRepository.ToggleStatus(id);
            TempData["SuccessMessage"] = "Question status updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _questionRepository.Delete(id);
            TempData["SuccessMessage"] = "Question deleted.";
            return RedirectToAction(nameof(Index));
        }

        private static List<string> GetAvailableQuestionTypes()
        {
            return new List<string>
            {
                "Rating5",
                "Rating10",
                "SingleChoice",
                "MultipleChoice",
                "YesNo",
                "Dropdown",
                "Text",
                "Textarea"
            };
        }
    }
}
