using Microsoft.AspNetCore.Mvc;
using Shared.Dtos;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PingController : ControllerBase
{
    [HttpGet]
    public ActionResult<PingResponseDto> Get() => Ok(new PingResponseDto("pong"));
}