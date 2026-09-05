namespace OnlineFeedbackSystem.Models.Entities
{
    public class FormQuestions
    {
        public int Id { get; set; }
        public int FormId { get; set; }
        public int QuestionId { get; set; }
        public bool IsRequired { get; set; } = true;
        public int DisplayOrder { get; set; } = 1;

        // Navigation reference to full question
        public Questions? Question { get; set; }
    }
}
