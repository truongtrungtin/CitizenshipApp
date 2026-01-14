namespace Shared.Contracts.Deck;

public static class QuestionTypeMapper
{
    public static QuestionType FromRaw(string? raw)
    {
        string s = (raw ?? string.Empty).Trim().ToUpperInvariant();

        return s switch
        {
            "TEXT" => QuestionType.Text,
            "MCQ" => QuestionType.SingleChoice,
            "SINGLE" => QuestionType.SingleChoice,
            "SINGLECHOICE" => QuestionType.SingleChoice,
            "MULTI" => QuestionType.MultiChoice,
            "MULTICHOICE" => QuestionType.MultiChoice,
            _ => QuestionType.Unknown
        };
    }
}
