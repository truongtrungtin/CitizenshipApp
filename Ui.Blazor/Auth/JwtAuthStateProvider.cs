using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Ui.Blazor.Services;

namespace Ui.Blazor.Auth;

public sealed class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly ITokenStore _tokenStore;

    public JwtAuthStateProvider(ITokenStore tokenStore) => _tokenStore = tokenStore;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _tokenStore.GetAccessTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var claims = JwtParser.ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, authenticationType: "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public void NotifyUserAuthentication(string token)
    {
        var claims = JwtParser.ParseClaimsFromJwt(token);
        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(authenticatedUser)));
    }

    public void NotifyUserLogout()
    {
        var anon = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anon)));
    }
}
