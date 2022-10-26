namespace Avtotest.Web.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string? QuestionText { get; set; }
        public string? Description { get; set; }
        public List<Choice>? Choices { get; set; }
        public string? Image { get; set; }
    }
}
