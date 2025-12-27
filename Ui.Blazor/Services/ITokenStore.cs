namespace Ui.Blazor.Services;

public interface ITokenStore
{
    ValueTask<string?> GetAccessTokenAsync();
    ValueTask SetAccessTokenAsync(string token);
    ValueTask ClearAsync();
}
