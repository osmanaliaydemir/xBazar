using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API.Swagger;

public class ApiResponseOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Ensure JSON content exists and attach example payloads
        AddOrUpdateResponse(operation, "200", new OpenApiObject
        {
            ["isSuccess"] = new OpenApiBoolean(true),
            ["message"] = new OpenApiString("OK"),
            ["data"] = new OpenApiObject(),
            ["errors"] = new OpenApiArray(),
            ["correlationId"] = new OpenApiString("{trace-id}"),
            ["timestamp"] = new OpenApiString(DateTime.UtcNow.ToString("o"))
        });

        AddOrUpdateResponse(operation, "400", new OpenApiObject
        {
            ["isSuccess"] = new OpenApiBoolean(false),
            ["message"] = new OpenApiString("Validation error"),
            ["errors"] = new OpenApiArray { new OpenApiString("One or more validation errors occurred.") },
            ["correlationId"] = new OpenApiString("{trace-id}"),
            ["timestamp"] = new OpenApiString(DateTime.UtcNow.ToString("o")),
            ["statusCode"] = new OpenApiInteger(400)
        });

        AddOrUpdateResponse(operation, "401", new OpenApiObject
        {
            ["isSuccess"] = new OpenApiBoolean(false),
            ["message"] = new OpenApiString("Unauthorized"),
            ["errors"] = new OpenApiArray(),
            ["correlationId"] = new OpenApiString("{trace-id}"),
            ["timestamp"] = new OpenApiString(DateTime.UtcNow.ToString("o")),
            ["statusCode"] = new OpenApiInteger(401)
        });

        AddOrUpdateResponse(operation, "404", new OpenApiObject
        {
            ["isSuccess"] = new OpenApiBoolean(false),
            ["message"] = new OpenApiString("Not found"),
            ["errors"] = new OpenApiArray(),
            ["correlationId"] = new OpenApiString("{trace-id}"),
            ["timestamp"] = new OpenApiString(DateTime.UtcNow.ToString("o")),
            ["statusCode"] = new OpenApiInteger(404)
        });
    }

    private static void AddOrUpdateResponse(OpenApiOperation operation, string statusCode, IOpenApiAny example)
    {
        if (!operation.Responses.TryGetValue(statusCode, out var response))
        {
            response = new OpenApiResponse { Description = GetDescription(statusCode) };
            operation.Responses[statusCode] = response;
        }

        if (response.Content == null)
        {
            response.Content = new Dictionary<string, OpenApiMediaType>();
        }

        response.Content["application/json"] = new OpenApiMediaType
        {
            Example = example
        };
    }

    private static string GetDescription(string statusCode)
    {
        return statusCode switch
        {
            "200" => "Success",
            "400" => "Bad Request",
            "401" => "Unauthorized",
            "404" => "Not Found",
            _ => statusCode
        };
    }
}


