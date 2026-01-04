using Microsoft.AspNetCore.Mvc;

using Shared.Contracts.Ping;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PingController : ControllerBase
{
    [HttpGet]
    public ActionResult<PingResponse> Get()
    {
        return Ok(new PingResponse("pong"));
    }
}
