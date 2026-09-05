namespace OnlineFeedbackSystem.Models.ViewModels
{
    public class ResponseListViewModel
    {
        public int? SelectedFormId { get; set; }
        public List<FeedbackFormListItemViewModel> AvailableForms { get; set; } = new();
        public List<ResponseListItemViewModel> Responses { get; set; } = new();
    }

    public class ResponseListItemViewModel
    {
        public int Id { get; set; }
        public int FormId { get; set; }
        public string FormTitle { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string RespondentName { get; set; } = string.Empty;
        public string SubmissionToken { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public bool IsAnonymous { get; set; }
        public double? AverageRating { get; set; }
    }

    public class ResponseDetailViewModel
    {
        public int ResponseId { get; set; }
        public int FormId { get; set; }
        public string FormTitle { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string RespondentName { get; set; } = string.Empty;
        public string SubmissionToken { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public bool IsAnonymous { get; set; }
        public List<ResponseAnswerDetailViewModel> Answers { get; set; } = new();
    }

    public class ResponseAnswerDetailViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public string? AnswerText { get; set; }
        public double? NumericValue { get; set; }
    }
}
