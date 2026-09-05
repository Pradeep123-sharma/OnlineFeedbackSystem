namespace OnlineFeedbackSystem.Models.Entities
{
    public class FeedbackAnswers
    {
        public int Id { get; set; }
        public int ResponseId { get; set; }
        public int QuestionId { get; set; }
        public string? AnswerText { get; set; }
        public double? NumericValue { get; set; }
    }
}
