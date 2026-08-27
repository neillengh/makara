namespace Makara.Core.Models;

public class FieldMapping
{
    public string QuestionField { get; set; } = string.Empty;
    public string AnswerField { get; set; } = string.Empty;
    public string? InputField { get; set; }
    public string? SystemPrompt { get; set; }
}
