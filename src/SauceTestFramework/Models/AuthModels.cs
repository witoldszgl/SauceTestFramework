namespace SauceTestFramework.Models;

/// <summary>
/// Response from the restful-booker /auth endpoint.
/// </summary>
public sealed record AuthTokenResponse(string Token);

/// <summary>
/// Request payload for restful-booker authentication.
/// </summary>
public sealed record AuthTokenRequest(string Username, string Password);
