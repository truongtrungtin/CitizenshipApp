using System.Net.Http.Headers;

namespace Ui.Blazor.Services;

/// <summary>
/// Automatically attaches Authorization: Bearer {token} for API calls.
/// Reads token from AuthSession (singleton) so it works across handler scopes.
/// </summary>
public sealed class AuthHeaderHandler(AuthSession session) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (session.IsAuthenticated && request.Headers.Authorization is null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
