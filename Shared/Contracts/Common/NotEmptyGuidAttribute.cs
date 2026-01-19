using System;
using System.ComponentModel.DataAnnotations;

namespace Shared.Contracts.Common;

/// <summary>
/// Validates that a Guid is not Guid.Empty.
/// Why: [Required] does not treat Guid.Empty as invalid.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class NotEmptyGuidAttribute : ValidationAttribute
{
    public NotEmptyGuidAttribute()
        : base("The {0} field must not be empty.")
    {
    }

    public override bool IsValid(object? value)
    {
        // Null is invalid for Guid fields in requests (unless you use Guid?).
        if (value is null) return false;

        // Only validate Guid values.
        return value is Guid guid && guid != Guid.Empty;
    }
}
