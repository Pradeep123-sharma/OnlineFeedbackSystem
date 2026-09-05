using System.ComponentModel.DataAnnotations;

namespace OnlineFeedbackSystem.Models.ViewModels
{
    public class QuestionListItemViewModel
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public List<string> Options { get; set; } = new();
    }

    public class QuestionBankIndexViewModel
    {
        public List<QuestionListItemViewModel> Questions { get; set; } = new();
        public string? SelectedCategory { get; set; }
        public string? SelectedType { get; set; }
        public string? SearchQuery { get; set; }
        public List<string> Categories { get; set; } = new();
        public List<string> QuestionTypes { get; set; } = new();
    }

    public class QuestionCreateEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Question text is required.")]
        [StringLength(500, ErrorMessage = "Question text cannot exceed 500 characters.")]
        [Display(Name = "Question Text")]
        public string QuestionText { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a question type.")]
        [Display(Name = "Question Type")]
        public string QuestionType { get; set; } = "Rating5";

        [Required(ErrorMessage = "Category is required.")]
        [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters.")]
        [Display(Name = "Category")]
        public string Category { get; set; } = string.Empty;

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = true;

        // List of option texts passed from dynamic frontend option rows
        public List<string> Options { get; set; } = new();
    }
}
