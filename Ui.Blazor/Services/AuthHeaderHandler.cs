using System.Net.Http.Headers;

namespace Ui.Blazor.Services;

/// <summary>
///     Automatically attaches Authorization: Bearer {token} for API calls.
///     Keep ApiClient clean (no manual header injection scattered everywhere).
/// </summary>
public sealed class AuthHeaderHandler(AppState state) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (state.IsAuthenticated && request.Headers.Authorization is null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", state.AccessToken);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
