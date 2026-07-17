using System.Net.Http.Json;
using System.Text.Json;
using SauceTestFramework.Models;

namespace SauceTestFramework.Clients;

/// <summary>
/// Clean, reusable HTTP client wrapper for REST API interactions.
/// Handles JSON serialization/deserialization and common headers.
/// </summary>
public sealed class ApiClient : IDisposable
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOptions;

    private static readonly JsonSerializerOptions DefaultJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(string baseUrl, JsonSerializerOptions? jsonOptions = null)
    {
        _jsonOptions = jsonOptions ?? DefaultJsonOptions;
        _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _http.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
    }

    // ──────────────────────────── Generic CRUD ────────────────────────────

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        var response = await _http.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest payload)
    {
        var response = await _http.PostAsJsonAsync(endpoint, payload, _jsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
    }

    public async Task<HttpResponseMessage> PostAsync<TRequest>(string endpoint, TRequest payload)
    {
        var response = await _http.PostAsJsonAsync(endpoint, payload, _jsonOptions);
        response.EnsureSuccessStatusCode();
        return response;
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest payload)
    {
        var response = await _http.PutAsJsonAsync(endpoint, payload, _jsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
    }

    public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
    {
        var response = await _http.DeleteAsync(endpoint);
        response.EnsureSuccessStatusCode();
        return response;
    }

    // ──────────────────────── Domain-Specific Helpers ─────────────────────

    /// <summary>
    /// Authenticates with the restful-booker API and returns a session token.
    /// </summary>
    public async Task<string> GetBookerAuthTokenAsync(string username, string password)
    {
        var request = new AuthTokenRequest(username, password);
        var response = await PostAsync<AuthTokenRequest, AuthTokenResponse>("auth", request);
        return response?.Token ?? throw new InvalidOperationException("Auth token was null.");
    }

    /// <summary>
    /// Creates a new booking via the restful-booker API.
    /// </summary>
    public async Task<BookingResponse> CreateBookingAsync(Booking booking)
    {
        var response = await PostAsync<Booking, BookingResponse>("booking", booking);
        return response ?? throw new InvalidOperationException("Booking creation failed.");
    }

    /// <summary>
    /// Retrieves a single booking by its ID from the restful-booker API.
    /// </summary>
    public async Task<Booking> GetBookingByIdAsync(int bookingId)
    {
        var response = await GetAsync<Booking>($"booking/{bookingId}");
        return response ?? throw new InvalidOperationException($"Booking {bookingId} not found.");
    }

    /// <summary>
    /// Sets a bearer/cookie token header for subsequent authenticated requests.
    /// </summary>
    public void SetCookieToken(string token)
    {
        _http.DefaultRequestHeaders.Add("Cookie", $"token={token}");
    }

    public void Dispose() => _http.Dispose();
}

