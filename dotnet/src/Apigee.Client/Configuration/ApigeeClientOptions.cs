using System.ComponentModel.DataAnnotations;

namespace Apigee.Client.Configuration;

/// <summary>
/// Configuration required to authenticate and communicate with an Apigee-fronted API.
/// Bind this from configuration (e.g. the "Apigee" section of appsettings.json) or
/// build it programmatically via one of the <c>AddApigeeClient</c> extension method overloads.
/// </summary>
public sealed class ApigeeClientOptions
{
    /// <summary>Default configuration section name used by the <c>IConfiguration</c>-based overload.</summary>
    public const string SectionName = "Apigee";

    /// <summary>Absolute base address of the Apigee proxy that fronts the target API.</summary>
    [Required(AllowEmptyStrings = false)]
    [Url]
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Absolute URL of the OAuth2 token endpoint used for the client-credentials grant.</summary>
    [Required(AllowEmptyStrings = false)]
    [Url]
    public string TokenEndpoint { get; set; } = string.Empty;

    /// <summary>Client id issued by Apigee for this microservice.</summary>
    [Required(AllowEmptyStrings = false)]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>Client secret issued by Apigee for this microservice. Supply this from a secret store, never source control.</summary>
    [Required(AllowEmptyStrings = false)]
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>Optional OAuth2 scope(s) to request, space-separated.</summary>
    public string? Scope { get; set; }

    /// <summary>Per-request timeout, in seconds, applied to both the token and API HTTP clients.</summary>
    [Range(1, 300)]
    public int RequestTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Safety margin, in seconds, subtracted from the token's reported expiry so a refresh
    /// happens slightly before the token actually expires.
    /// </summary>
    [Range(0, 60)]
    public int TokenExpiryBufferSeconds { get; set; } = 60;

    /// <summary>Number of retry attempts the resilience pipeline performs for transient failures.</summary>
    [Range(0, 10)]
    public int RetryCount { get; set; } = 3;
}
