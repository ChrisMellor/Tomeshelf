using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.ComicConApi.Enums;

namespace Tomeshelf.ComicConApi.Transformers;

/// <summary>
/// OpenAPI schema transformer that renders the City enum as string values.
/// </summary>
public class CitySchemaTransformer : IOpenApiSchemaTransformer
{
    /// <summary>
    /// Transforms the OpenAPI schema for the <see cref="City"/> enum to a string enum with named values.
    /// </summary>
    /// <param name="schema">The schema to mutate when the target type matches.</param>
    /// <param name="context">Provides the target JSON type.</param>
    /// <param name="cancellationToken">Cancellation token for the transformer pipeline.</param>
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;

        if (type != typeof(City))
        {
            return Task.CompletedTask;
        }

        schema.Type = "string";
        schema.Format = null;
        schema.Enum = Enum.GetNames<City>()
            .Select(n => (IOpenApiAny)new OpenApiString(n))
            .ToList();

        return Task.CompletedTask;
    }
}
