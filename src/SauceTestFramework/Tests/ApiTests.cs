using FluentAssertions;
using SauceTestFramework.Clients;
using SauceTestFramework.Config;
using SauceTestFramework.Models;

namespace SauceTestFramework.Tests;

/// <summary>
/// Pure API tests exercising restful-booker endpoints.
/// These tests do NOT require Playwright — they run headlessly via HttpClient.
/// </summary>
public sealed class ApiTests : IDisposable
{
    private readonly ApiClient _bookerClient;
    private readonly ApiSettings _apiSettings;

    public ApiTests()
    {
        var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var settings = File.Exists(settingsPath)
            ? System.Text.Json.JsonSerializer.Deserialize<TestSettings>(
                File.ReadAllText(settingsPath),
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
              ?? new TestSettings()
            : new TestSettings();

        _apiSettings = settings.Api;
        _bookerClient = new ApiClient(_apiSettings.BookerBaseUrl);
    }

    [Fact]
    public async Task BookerAuth_WithValidCredentials_ShouldReturnToken()
    {
        // Act
        var token = await _bookerClient.GetBookerAuthTokenAsync(
            _apiSettings.BookerUsername,
            _apiSettings.BookerPassword);

        // Assert
        token.Should().NotBeNullOrWhiteSpace("a valid auth request should yield a token");
    }

    [Fact]
    public async Task BookerCreateBooking_ShouldReturnBookingId()
    {
        // Arrange
        var booking = new Booking(
            Firstname: "James",
            Lastname: "Bond",
            Totalprice: 250,
            Depositpaid: true,
            Bookingdates: new BookingDates("2026-08-01", "2026-08-10"),
            Additionalneeds: "License to kill");

        // Act
        var response = await _bookerClient.CreateBookingAsync(booking);

        // Assert
        response.BookingId.Should().BePositive();
        response.Booking.Firstname.Should().Be("James");
        response.Booking.Lastname.Should().Be("Bond");
        response.Booking.Totalprice.Should().Be(250);
    }

    [Fact]
    public async Task BookerGetBookingIds_ShouldReturnNonEmptyList()
    {
        // Act
        var ids = await _bookerClient.GetAsync<List<BookingIdEntry>>("booking");

        // Assert
        ids.Should().NotBeNullOrEmpty("there should always be at least one booking");
    }

    [Fact]
    public async Task BookerGetBookingById_ShouldReturnBookingData()
    {
        // Arrange — create a booking first so we have a known ID
        var booking = new Booking(
            Firstname: "Ada",
            Lastname: "Lovelace",
            Totalprice: 150,
            Depositpaid: true,
            Bookingdates: new BookingDates("2026-09-01", "2026-09-05"));

        var created = await _bookerClient.CreateBookingAsync(booking);

        // Act
        var fetched = await _bookerClient.GetBookingByIdAsync(created.BookingId);

        // Assert
        fetched.Firstname.Should().Be("Ada");
        fetched.Lastname.Should().Be("Lovelace");
        fetched.Totalprice.Should().Be(150);
    }

    public void Dispose() => _bookerClient.Dispose();
}
