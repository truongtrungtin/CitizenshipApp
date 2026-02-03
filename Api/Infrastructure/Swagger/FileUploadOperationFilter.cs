using Microsoft.OpenApi;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api.Infrastructure.Swagger;

public sealed class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileParams = context.ApiDescription.ParameterDescriptions
            .Where(p => p.Type == typeof(IFormFile))
            .ToList();

        if (fileParams.Count == 0)
        {
            return;
        }

        operation.Parameters = new List<IOpenApiParameter>();

        var schema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>()
        };

        foreach (var param in fileParams)
        {
            schema.Properties[param.Name] = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Format = "binary"
            };
        }

        operation.RequestBody = new OpenApiRequestBody
        {
            Required = true,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = schema
                }
            }
        };
    }
}
