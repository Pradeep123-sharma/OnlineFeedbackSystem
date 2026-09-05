namespace OnlineFeedbackSystem.Models.Entities
{
    public class QuestionOptions
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string OptionText { get; set; } = string.Empty;
        public string OptionValue { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }
}
