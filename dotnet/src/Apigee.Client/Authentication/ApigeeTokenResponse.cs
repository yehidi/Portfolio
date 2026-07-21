using System.Text.Json.Serialization;

namespace Apigee.Client.Authentication;

/// <summary>Shape of the JSON payload returned by Apigee's OAuth2 token endpoint.</summary>
internal sealed record ApigeeTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("expires_in")] int ExpiresInSeconds,
    [property: JsonPropertyName("token_type")] string? TokenType);

/// <summary>Source-generated serialization context, avoiding reflection-based JSON (de)serialization.</summary>
[JsonSerializable(typeof(ApigeeTokenResponse))]
internal sealed partial class ApigeeTokenJsonContext : JsonSerializerContext
{
}
