namespace Ui.Blazor.Services;

public sealed class AppState
{
    public bool IsLoggedIn { get; private set; }
    public bool HasCompletedOnboarding { get; private set; }

    public event Action? OnChange;

    public void SignIn(bool onboardingDone = false)
    {
        IsLoggedIn = true;
        HasCompletedOnboarding = onboardingDone;
        OnChange?.Invoke();
    }

    public void CompleteOnboarding()
    {
        HasCompletedOnboarding = true;
        OnChange?.Invoke();
    }

    public void SignOut()
    {
        IsLoggedIn = false;
        HasCompletedOnboarding = false;
        OnChange?.Invoke();
    }
}
