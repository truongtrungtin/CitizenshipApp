namespace Ui.Blazor.Shared;

/// <summary>
/// Gắn attribute này lên page để cho phép truy cập dù user chưa onboarded.
/// Ví dụ: /onboarding, hoặc các trang public sau login.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class SkipOnboardingGuardAttribute : Attribute
{
}
