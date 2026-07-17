using System.Text.Json.Serialization;

namespace SauceTestFramework.Models;

/// <summary>
/// Models for reqres.in user API responses.
/// </summary>
public sealed record ReqresUserResponse(
    [property: JsonPropertyName("data")] ReqresUser Data,
    [property: JsonPropertyName("support")] ReqresSupport Support);

public sealed record ReqresUser(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("first_name")] string FirstName,
    [property: JsonPropertyName("last_name")] string LastName,
    [property: JsonPropertyName("avatar")] string Avatar);

public sealed record ReqresSupport(
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("text")] string Text);

/// <summary>
/// Response from reqres.in POST /login or /register.
/// </summary>
public sealed record ReqresLoginResponse(
    [property: JsonPropertyName("token")] string Token);

public sealed record ReqresLoginRequest(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("password")] string Password);
