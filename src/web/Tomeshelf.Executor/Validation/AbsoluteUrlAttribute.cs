using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Tomeshelf.Executor.Validation;

/// <summary>
///     Validates that a string contains an absolute URL with an allowed scheme.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class AbsoluteUrlAttribute : ValidationAttribute, IClientModelValidator
{
    private static readonly string[] DefaultSchemes = ["http", "https", "ftp"];
    private static readonly HashSet<string> AllowedSchemes = new HashSet<string>(DefaultSchemes, StringComparer.OrdinalIgnoreCase);

    public AbsoluteUrlAttribute() : base("The {0} field must be a fully-qualified http, https, or ftp URL.") { }

    public void AddValidation(ClientModelValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-absoluteurl", FormatErrorMessage(context.ModelMetadata.GetDisplayName() ?? context.ModelMetadata.Name ?? "This field"));
        MergeAttribute(context.Attributes, "data-val-absoluteurl-schemes", string.Join(",", DefaultSchemes));
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        if (value is not string stringValue)
        {
            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
        }

        var trimmed = stringValue.Trim();
        if (trimmed.Length == 0)
        {
            return ValidationResult.Success;
        }

        if (!TryCreateAbsolute(trimmed, out var uri))
        {
            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
        }

        if (!AllowedSchemes.Contains(uri.Scheme))
        {
            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
        }

        return ValidationResult.Success;
    }

    private static bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
    {
        if (attributes.ContainsKey(key))
        {
            return false;
        }

        attributes.Add(key, value);

        return true;
    }

    private static bool TryCreateAbsolute(string value, [NotNullWhen(true)] out Uri? uri)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out uri) && AllowedSchemes.Contains(uri.Scheme))
        {
            return true;
        }

        if (!value.Contains("://", StringComparison.Ordinal))
        {
            var candidate = $"http://{value}";
            if (Uri.TryCreate(candidate, UriKind.Absolute, out uri) && AllowedSchemes.Contains(uri.Scheme))
            {
                return true;
            }
        }

        uri = null;

        return false;
    }
}