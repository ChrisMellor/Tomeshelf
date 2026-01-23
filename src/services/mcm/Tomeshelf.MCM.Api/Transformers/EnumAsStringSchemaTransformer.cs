using System;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Tomeshelf.Mcm.Api.Transformers;

/// <summary>
///     Transforms OpenAPI schemas for .NET enum types to represent them as strings in the generated schema.
/// </summary>
/// <remarks>
///     This transformer modifies the OpenAPI schema so that enums are described as string values rather than
///     their underlying numeric values. For enums marked with the <see cref="FlagsAttribute" />, the schema will indicate
///     a string type without enumerating possible values. This class is intended for internal use in schema generation
///     pipelines and is not thread-safe.
/// </remarks>
internal sealed class EnumAsStringSchemaTransformer : IOpenApiSchemaTransformer
{
    /// <summary>
    ///     Transforms the provided OpenAPI schema to represent an enumeration type as a string or as a set of named values,
    ///     based on the associated .NET type information.
    /// </summary>
    /// <remarks>
    ///     If the .NET type is an enumeration with the Flags attribute, the schema is set to type string
    ///     without enumerating possible values. For standard enumerations, the schema's enum values are set to the names of
    ///     the enumeration members. Non-enum types are not modified.
    /// </remarks>
    /// <param name="schema">The OpenAPI schema to modify to reflect the enumeration type.</param>
    /// <param name="context">
    ///     The transformation context containing type information used to determine how the schema should be
    ///     updated.
    /// </param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous transformation operation.</returns>
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;
        type = Nullable.GetUnderlyingType(type) ?? type;

        if (!type.IsEnum)
        {
            return Task.CompletedTask;
        }

        var isFlags = Attribute.IsDefined(type, typeof(FlagsAttribute), false);

        schema.Type = JsonSchemaType.String;
        schema.Format = null;

        if (isFlags)
        {
            schema.Enum = null;

            return Task.CompletedTask;
        }

        var names = Enum.GetNames(type);

        var arr = new JsonArray();
        foreach (var name in names)
        {
            arr.Add(name);
        }

        schema.Enum = arr;

        return Task.CompletedTask;
    }
}