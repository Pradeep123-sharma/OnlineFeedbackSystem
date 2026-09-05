namespace OnlineFeedbackSystem.Models.ViewModels
{
    public class RespondentAvailableFormItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsAnonymous { get; set; }
        public bool AlreadySubmitted { get; set; }
        public int QuestionCount { get; set; }
    }

    public class RespondentFormFillingViewModel
    {
        public int FormId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public bool IsAnonymous { get; set; }
        public bool OneResponseOnly { get; set; }

        // Form questions list
        public List<RespondentQuestionInputViewModel> Questions { get; set; } = new();
    }

    public class RespondentQuestionInputViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }
        public List<string> Options { get; set; } = new();

        // Respondent answers to bind
        public string? TextAnswer { get; set; }
        public double? NumericAnswer { get; set; }
        public List<string> SelectedChoices { get; set; } = new();
    }

    public class SubmissionConfirmationViewModel
    {
        public string SubmissionToken { get; set; } = string.Empty;
        public string FormTitle { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public bool IsAnonymous { get; set; }
    }

    public class RespondentDashboardViewModel
    {
        public string RespondentName { get; set; } = "Respondent";
        public bool IsAuthenticated { get; set; }
        public int TotalAvailableSurveys { get; set; }
        public int CompletedSurveys { get; set; }
        public int PendingSurveys { get; set; }
        public List<RespondentAvailableFormItemViewModel> AvailableForms { get; set; } = new();
        public List<RespondentSubmissionHistoryItemViewModel> RecentSubmissions { get; set; } = new();
    }

    public class RespondentSubmissionHistoryItemViewModel
    {
        public int ResponseId { get; set; }
        public int FormId { get; set; }
        public string FormTitle { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public string SubmissionToken { get; set; } = string.Empty;
        public string Status { get; set; } = "Submitted";
    }
}
