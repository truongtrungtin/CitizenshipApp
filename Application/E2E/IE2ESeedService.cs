using Shared.Contracts.E2E;

namespace Application.E2E;

public interface IE2ESeedService
{
    Task<E2ESeedResponse> SeedAsync(CancellationToken ct);
}
