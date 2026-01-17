using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Shared.Contracts.Common;

/// <summary>
/// Validates a username that can be either:
/// - Email (contains '@') -> EmailAddressAttribute
/// - Phone number (digits, may start with '+') -> basic regex
///
/// Why: The project allows using email OR phone as login/register "username".
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class EmailOrPhoneAttribute : ValidationAttribute
{
    private static readonly EmailAddressAttribute EmailValidator = new();
    private static readonly Regex PhoneRegex = new(@"^\+?[0-9]{7,15}$", RegexOptions.Compiled);

    public EmailOrPhoneAttribute()
        : base("The {0} field must be a valid email address or phone number.")
    {
    }

    public override bool IsValid(object? value)
    {
        if (value is null) return false;

        if (value is not string s) return false;

        s = s.Trim();
        if (s.Length == 0) return false;

        // If it looks like email, validate as email.
        if (s.Contains('@'))
        {
            return EmailValidator.IsValid(s);
        }

        // Otherwise, validate as phone.
        return PhoneRegex.IsMatch(s);
    }
}