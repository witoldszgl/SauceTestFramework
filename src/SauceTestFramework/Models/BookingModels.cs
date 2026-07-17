using System.Text.Json.Serialization;

namespace SauceTestFramework.Models;

/// <summary>
/// Booking data model for restful-booker.
/// </summary>
public sealed record Booking(
    string Firstname,
    string Lastname,
    int Totalprice,
    bool Depositpaid,
    BookingDates Bookingdates,
    string? Additionalneeds = null);

public sealed record BookingDates(string Checkin, string Checkout);

/// <summary>
/// Response wrapper returned when creating a booking.
/// </summary>
public sealed record BookingResponse(
    [property: JsonPropertyName("bookingid")] int BookingId,
    [property: JsonPropertyName("booking")] Booking Booking);

/// <summary>
/// Lightweight booking ID entry from GET /booking.
/// </summary>
public sealed record BookingIdEntry(
    [property: JsonPropertyName("bookingid")] int BookingId);
