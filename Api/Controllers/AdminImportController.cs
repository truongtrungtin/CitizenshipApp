using System.Text.Json;

using Application.AdminImport;
using Application.AdminImport.Parsers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Shared.Contracts.AdminImport;

namespace Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public sealed class AdminImportController(IQuestionImportService service) : ControllerBase
{
    public sealed class AdminImportCsvForm
    {
        public IFormFile? File { get; init; }
    }

    /// <summary>
    ///     POST /api/admin/import/questions/json
    /// </summary>
    [HttpPost("import/questions/json")]
    public async Task<ActionResult<AdminImportQuestionsResult>> ImportQuestionsJson(
        [FromBody] AdminImportQuestionsRequest req,
        CancellationToken ct)
    {
        if (req.Items.Count == 0)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                detail: "Items is required.");
        }

        AdminImportQuestionsResult result = await service.ImportAsync(req.Items, null, ct);
        return Ok(result);
    }

    /// <summary>
    ///     POST /api/admin/import/questions
    ///     Accepts application/json, text/csv, or multipart/form-data.
    /// </summary>
    [HttpPost("import/questions")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<ActionResult<AdminImportQuestionsResult>> ImportQuestions(CancellationToken ct)
    {
        string? contentType = Request.ContentType;
        if (contentType is null)
        {
            return Problem(
                statusCode: StatusCodes.Status415UnsupportedMediaType,
                title: "Unsupported Media Type",
                detail: "Content-Type is required.");
        }

        if (contentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
        {
            AdminImportQuestionsRequest? req = await JsonSerializer.DeserializeAsync<AdminImportQuestionsRequest>(
                Request.Body,
                new JsonSerializerOptions(JsonSerializerDefaults.Web),
                ct);

            if (req is null || req.Items.Count == 0)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Bad Request",
                    detail: "Items is required.");
            }

            AdminImportQuestionsResult result = await service.ImportAsync(req.Items, null, ct);
            return Ok(result);
        }

        if (contentType.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase))
        {
            IFormFile? file = (await Request.ReadFormAsync(ct)).Files.FirstOrDefault();
            return await ImportCsvFileAsync(file, ct);
        }

        if (contentType.StartsWith("text/csv", StringComparison.OrdinalIgnoreCase) ||
            contentType.StartsWith("application/csv", StringComparison.OrdinalIgnoreCase))
        {
            return await ImportCsvStreamAsync(Request.Body, ct);
        }

        return Problem(
            statusCode: StatusCodes.Status415UnsupportedMediaType,
            title: "Unsupported Media Type",
            detail: "Supported: application/json, text/csv, multipart/form-data.");
    }

    /// <summary>
    ///     POST /api/admin/import/questions/csv
    /// </summary>
    [HttpPost("import/questions/csv")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<AdminImportQuestionsResult>> ImportQuestionsCsv(
        [FromForm] AdminImportCsvForm form,
        CancellationToken ct)
    {
        return await ImportCsvFileAsync(form.File, ct);
    }

    private async Task<ActionResult<AdminImportQuestionsResult>> ImportCsvFileAsync(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                detail: "CSV file is required.");
        }

        await using Stream stream = file.OpenReadStream();
        return await ImportCsvStreamAsync(stream, ct);
    }

    private async Task<ActionResult<AdminImportQuestionsResult>> ImportCsvStreamAsync(Stream stream, CancellationToken ct)
    {
        CsvParseResult parsed;
        try
        {
            parsed = await new CsvQuestionImportParser().ParseAsync(stream, ct);
        }
        catch (Exception ex)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                detail: $"CSV parse failed: {ex.Message}");
        }

        AdminImportQuestionsResult result = await service.ImportAsync(
            parsed.Items,
            parsed.CsvLineByIndex,
            ct);

        return Ok(result);
    }
}
