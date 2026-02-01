using Application.AppSettings;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Shared.Contracts.AppSettings;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AppSettingsController(IAppSettingsService service) : ControllerBase
{
    [HttpGet]
    public async Task<List<AppSettingDto>> GetAll(CancellationToken ct)
    {
        return await service.GetAllAsync(ct);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AppSettingDto>> Get(int id, CancellationToken ct)
    {
        AppSettingDto? item = await service.GetByIdAsync(id, ct);
        return item is null
            ? Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found",
                detail: "App setting not found.")
            : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<AppSettingDto>> Create([FromBody] CreateAppSettingRequest req, CancellationToken ct)
    {
        AppSettingResult result = await service.CreateAsync(req, ct);
        return MapResult(result, "Create failed.");
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAppSettingRequest req, CancellationToken ct)
    {
        AppSettingResult result = await service.UpdateAsync(id, req, ct);
        return MapResultAsAction(result, "Update failed.");
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        AppSettingResult result = await service.DeleteAsync(id, ct);
        return MapResultAsAction(result, "Delete failed.");
    }

    private ActionResult<AppSettingDto> MapResult(AppSettingResult result, string defaultError)
    {
        if (result.Succeeded)
        {
            return result.Item is null
                ? Ok()
                : Ok(result.Item);
        }

        return result.FailureReason switch
        {
            AppSettingFailureReason.Conflict => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflict",
                detail: result.ErrorMessage),
            AppSettingFailureReason.NotFound => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found",
                detail: result.ErrorMessage),
            _ => Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Internal Server Error",
                detail: defaultError)
        };
    }

    private IActionResult MapResultAsAction(AppSettingResult result, string defaultError)
    {
        if (result.Succeeded)
        {
            return result.Item is null ? NoContent() : Ok(result.Item);
        }

        return result.FailureReason switch
        {
            AppSettingFailureReason.Conflict => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflict",
                detail: result.ErrorMessage),
            AppSettingFailureReason.NotFound => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found",
                detail: result.ErrorMessage),
            _ => Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Internal Server Error",
                detail: defaultError)
        };
    }
}
