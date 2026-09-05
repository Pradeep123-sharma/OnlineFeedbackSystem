namespace OnlineFeedbackSystem.Models.Entities
{
    public class Questions
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // Navigation collection of options
        public List<QuestionOptions> Options { get; set; } = new();
    }
}
