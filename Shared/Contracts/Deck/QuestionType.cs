namespace Shared.Contracts.Deck;

public enum QuestionType
{
    /// <summary>
    ///     Unknown / not set.
    ///     Why: Keep 0 as a safe default value for enums when serialized/deserialized.
    /// </summary>
    Unknown = 0,

    /// <summary>
    ///     One correct option (A/B/C/D...).
    /// </summary>
    SingleChoice = 1,

    /// <summary>
    ///     Multiple correct options.
    /// </summary>
    MultiChoice = 2,

    /// <summary>
    ///     Free-text answer.
    /// </summary>
    Text = 3
}
