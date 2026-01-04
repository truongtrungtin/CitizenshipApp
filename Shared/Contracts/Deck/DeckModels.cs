using System;
using System.Collections.Generic;

namespace Shared.Contracts.Decks;

// NOTE:
// These types were previously located under Shared.Dtos.*.
// The project convention is now: Shared.Contracts is the ONLY place for HTTP request/response shapes.
// We keep the same wire format (names + structure) to avoid breaking the API/UI while we refactor.

/// <summary>
/// Summary info for a deck (used in list screens).
/// </summary>
public sealed record DeckListItemDto(Guid Id, string Code, string Name, int QuestionCount);

/// <summary>
/// A single answer option for a question.
/// </summary>
public sealed record AnswerOptionDto(string Key, string TextEn, string TextVi);

/// <summary>
/// Localized question text.
/// </summary>
public sealed record QuestionTextDto(string En, string Vi);

/// <summary>
/// A question and its options.
/// </summary>
public sealed record QuestionDto(
    Guid Id,
    string DeckCode,
    QuestionTextDto Text,
    List<AnswerOptionDto> Options,
    string AnswerKey);
