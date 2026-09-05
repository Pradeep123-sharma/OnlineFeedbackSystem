using System;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineFeedbackSystem.Models.Entities;
using OnlineFeedbackSystem.Models.ViewModels;
using OnlineFeedbackSystem.Repositories;

namespace OnlineFeedbackSystem.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class QuestionsController : Controller
    {
        private readonly IQuestionRepository _questionRepository;

        public QuestionsController(IQuestionRepository questionRepository)
        {
            _questionRepository = questionRepository;
        }

        [HttpGet]
        public IActionResult Create()
        {
            // if you need ViewBag data (categories/types) populate here before returning the view
            return View(new QuestionCreateEditViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(QuestionCreateEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // provide any view data required by the Create view
                return View(model);
            }

            // Normalize user input to the canonical type values used by the app / DB
            model.QuestionType = NormalizeQuestionType(model.QuestionType);

            var question = new Questions
            {
                QuestionText = model.QuestionText.Trim(),
                QuestionType = model.QuestionType,
                Category = model.Category?.Trim() ?? string.Empty,
                IsActive = model.IsActive
            };

            var options = (model.Options ?? new List<string>())
                .Where(o => !string.IsNullOrWhiteSpace(o))
                .Select((o, idx) => new QuestionOptions
                {
                    OptionText = o.Trim(),
                    OptionValue = o.Trim(),
                    DisplayOrder = idx + 1
                })
                .ToList();

            try
            {
                int id = _questionRepository.Create(question, options);
                TempData["SuccessMessage"] = "Question added to Question Bank successfully!";
                return RedirectToAction("Index", "QuestionBank");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error creating question: " + ex.Message);
                return View(model);
            }
        }

        private static string NormalizeQuestionType(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var v = input.Trim().ToLowerInvariant();

            return v switch
            {
                "1-5 rating" or "1-5" or "5-star" or "5 star" or "5star" or "Rating5" => "5-Star Rating",
                "1-10 rating" or "1-10" or "10-star" or "10 star" or "10star" or "Rating10" => "1-10 Rating",
                "single choice" or "SingleChoice" or "single" => "Single Choice",
                "multiple choice" or "MultipleChoice" or "multiple" => "Multiple Choice",
                "yes/no" or "YesNo" or "yes no" => "Yes/No",
                "Dropdown" => "Dropdown",
                "text" => "Text",
                "textarea" => "Textarea",
                _ => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(v.Replace("-", " ").Replace("_", " "))
                        .Replace(" ", string.Empty) switch
                {
                    var s when s.Equals("Rating5", StringComparison.OrdinalIgnoreCase) => "Rating5",
                    var s when s.Equals("Rating10", StringComparison.OrdinalIgnoreCase) => "Rating10",
                    _ => v // fallback: return raw lower-case input (you may choose to throw instead)
                }
            };
        }
    }
}