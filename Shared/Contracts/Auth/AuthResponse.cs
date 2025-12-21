namespace Shared.Contracts.Auth;

public sealed class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;

    // Nếu API bạn đã trả thêm fields (RefreshToken/Expires/IsOnboarded...) thì add vào đây.
}
