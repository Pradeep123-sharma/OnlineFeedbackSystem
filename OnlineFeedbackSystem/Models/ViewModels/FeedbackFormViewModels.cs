using System.ComponentModel.DataAnnotations;
using OnlineFeedbackSystem.Models.Entities;

namespace OnlineFeedbackSystem.Models.ViewModels
{
    public class FeedbackFormListItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsAnonymous { get; set; }
        public bool OneResponseOnly { get; set; }
        public string Status { get; set; } = "Draft";
        public int CreatedBy { get; set; }
        public int ResponseCount { get; set; }
        public int QuestionCount { get; set; }
    }

    public class FeedbackFormIndexViewModel
    {
        public List<FeedbackFormListItemViewModel> Forms { get; set; } = new();
        public string? SelectedStatus { get; set; }
        public string? SelectedCategory { get; set; }
        public int? SelectedDepartmentId { get; set; }
        public string? SearchQuery { get; set; }
        public List<string> Categories { get; set; } = new();
        public List<Departments> Departments { get; set; } = new();
    }

    public class FeedbackFormCreateEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Form title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters.")]
        [Display(Name = "Category")]
        public string Category { get; set; } = string.Empty;

        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public List<Departments> AvailableDepartments { get; set; } = new();

        [Required(ErrorMessage = "Start date is required.")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "End date is required.")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(14);

        public bool IsAnonymous { get; set; } = false;

        public bool OneResponseOnly { get; set; } = true;

        public string Status { get; set; } = "Draft";
    }

    public class FormBuilderViewModel
    {
        public int FormId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string Status { get; set; } = "Draft";

        // Questions already assigned to this form (form question viewmodel)
        public List<FormQuestionItemViewModel> AssignedQuestions { get; set; } = new();

        // Available questions from Question Bank to add (view model type)
        public List<QuestionListItemViewModel> AvailableBankQuestions { get; set; } = new();

        // Filters / helper values used by the builder view
        public string? BankSearchQuery { get; set; }
        public string? BankSelectedCategory { get; set; }
        public List<string> Categories { get; set; } = new();
    }

    public class FormQuestionItemViewModel
    {
        public int FormQuestionId { get; set; }
        public int FormId { get; set; }
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsRequired { get; set; } = true;
        public int DisplayOrder { get; set; }
        public List<string> Options { get; set; } = new();
    }

    public class FormQuestionOrderDto
    {
        public int FormQuestionId { get; set; }
        public int QuestionId { get; set; }
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class ReorderQuestionsRequestDto
    {
        public int FormId { get; set; }
        public List<int> OrderedQuestionIds { get; set; } = new();
    }
}
