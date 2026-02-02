using Application.E2E;

using Microsoft.AspNetCore.Mvc;

using Shared.Contracts.E2E;

namespace Api.Controllers;

[ApiController]
[Route("api/e2e")]
public sealed class E2EController(
    IE2ESeedService seed,
    IWebHostEnvironment env,
    IConfiguration configuration)
    : ControllerBase
{
    [HttpPost("seed")]
    public async Task<ActionResult<E2ESeedResponse>> Seed(CancellationToken ct)
    {
        if (!IsE2EEnabled())
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found",
                detail: "Endpoint is not available in this environment.");
        }

        E2ESeedResponse result = await seed.SeedAsync(ct);
        return Ok(result);
    }

    private bool IsE2EEnabled()
    {
        if (env.IsDevelopment())
        {
            return true;
        }

        return configuration.GetValue<bool>("E2E:Enabled");
    }
}
