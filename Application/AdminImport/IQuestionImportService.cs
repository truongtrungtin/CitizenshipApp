using Shared.Contracts.AdminImport;

namespace Application.AdminImport;

public interface IQuestionImportService
{
    Task<AdminImportQuestionsResult> ImportAsync(
        IReadOnlyList<AdminImportQuestionItem> items,
        IReadOnlyDictionary<int, int>? csvLineByIndex,
        CancellationToken ct);
}
