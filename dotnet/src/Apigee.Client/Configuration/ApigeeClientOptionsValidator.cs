using Microsoft.Extensions.Options;

namespace Apigee.Client.Configuration;

/// <summary>
/// Validates cross-field and format constraints on <see cref="ApigeeClientOptions"/> that
/// the <see cref="System.ComponentModel.DataAnnotations.UrlAttribute"/> alone cannot express
/// (an absolute, well-formed <see cref="Uri"/>).
/// </summary>
internal sealed class ApigeeClientOptionsValidator : IValidateOptions<ApigeeClientOptions>
{
    public ValidateOptionsResult Validate(string? name, ApigeeClientOptions options)
    {
        List<string>? failures = null;

        ValidateAbsoluteUri(options.BaseUrl, nameof(options.BaseUrl), ref failures);
        ValidateAbsoluteUri(options.TokenEndpoint, nameof(options.TokenEndpoint), ref failures);

        return failures is null
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static void ValidateAbsoluteUri(string? value, string propertyName, ref List<string>? failures)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            // Absence is already reported by the [Required] data annotation.
            return;
        }

        if (!Uri.TryCreate(value, UriKind.Absolute, out _))
        {
            (failures ??= []).Add($"{propertyName} ('{value}') must be an absolute URI.");
        }
    }
}
