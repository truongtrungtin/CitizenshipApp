using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;

namespace Api.Infrastructure;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected bool TryGetUserId(out Guid userId)
    {
        string? raw = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("sub")
                      ?? User.FindFirstValue(ClaimTypes.Name);

        return Guid.TryParse(raw, out userId);
    }
}
