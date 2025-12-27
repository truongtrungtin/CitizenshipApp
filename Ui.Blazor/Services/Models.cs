namespace Ui.Blazor.Services;

// Auth
public sealed record RegisterRequest(string Email, string Password);
public sealed record LoginRequest(string Email, string Password);

public sealed record AuthResponse(string AccessToken, bool IsOnboarded);

// Me
public sealed record MeProfileResponse(string UserId, string Email, bool IsOnboarded);

public sealed record MeSettingsResponse(string Language, int DailyGoal);

public sealed record UpdateMeSettingsRequest(string Language, int DailyGoal);
