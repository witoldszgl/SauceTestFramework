using FluentAssertions;
using SauceTestFramework.Fixtures;
using SauceTestFramework.Models;
using SauceTestFramework.Pages;

namespace SauceTestFramework.Tests;

/// <summary>
/// Hybrid tests demonstrating the API → UI transition pattern.
///
/// Instead of performing a slow, multi-step UI login, these tests:
///   1. Call a fast REST API to authenticate and/or retrieve session state.
///   2. Inject the resulting token/cookie directly into the Playwright BrowserContext.
///   3. Navigate straight to an authenticated page and perform UI assertions.
///
/// This dramatically reduces test execution time and decouples login mechanism
/// from the test scenarios that depend on an authenticated session.
/// </summary>
public sealed class HybridTests : BaseTest
{
    /// <summary>
    /// Demonstrates the full hybrid pattern:
    ///   • API call to restful-booker to prove authentication works at the API layer.
    ///   • Cookie injection into Playwright to bypass saucedemo's UI login.
    ///   • Direct navigation to the Inventory page with full UI verification.
    /// </summary>
    [Fact]
    public async Task HybridLogin_ApiAuthThenUiVerification_ShouldAccessInventory()
    {
        // ═══════════════════════════════════════════════════════════════
        // PHASE 1 — API: Authenticate via a fast REST call
        // ═══════════════════════════════════════════════════════════════
        using var apiClient = CreateApiClient(Settings.Api.BookerBaseUrl);
        var apiToken = await apiClient.GetBookerAuthTokenAsync(
            Settings.Api.BookerUsername,
            Settings.Api.BookerPassword);

        apiToken.Should().NotBeNullOrWhiteSpace(
            "the API authentication call should return a valid token");

        // ═══════════════════════════════════════════════════════════════
        // PHASE 2 — BRIDGE: Inject auth state into the browser context
        // ═══════════════════════════════════════════════════════════════
        //
        // saucedemo.com stores its session in a cookie named 'session-username'.
        // By injecting this directly, we skip the entire login form interaction.
        //
        // In a production system, this would typically be a JWT or session cookie
        // obtained from Phase 1's API call.
        await InjectCookieAsync(
            name: "session-username",
            value: Settings.Ui.DefaultUser,
            domain: "www.saucedemo.com",
            path: "/");

        // ═══════════════════════════════════════════════════════════════
        // PHASE 3 — UI: Navigate directly to the protected page & verify
        // ═══════════════════════════════════════════════════════════════
        var inventoryPage = new InventoryPage(Page);
        await inventoryPage.NavigateAsync();

        // Verify we landed on the inventory without going through login
        (await inventoryPage.IsOnInventoryPageAsync()).Should().BeTrue(
            "cookie injection should grant access without UI login");

        var title = await inventoryPage.GetPageTitleAsync();
        title.Should().Be("Products");

        var itemCount = await inventoryPage.GetInventoryItemCountAsync();
        itemCount.Should().Be(6, "the saucedemo inventory always has 6 products");
    }

    /// <summary>
    /// Hybrid flow: API authentication + UI shopping cart interaction.
    /// Proves the injected session supports full e-commerce operations.
    /// </summary>
    [Fact]
    public async Task HybridFlow_ApiAuthThenAddToCart_ShouldReflectInCartPage()
    {
        // Phase 1 — API auth (fast)
        using var bookerClient = CreateApiClient(Settings.Api.BookerBaseUrl);
        var bookerToken = await bookerClient.GetBookerAuthTokenAsync(
            Settings.Api.BookerUsername,
            Settings.Api.BookerPassword);

        bookerToken.Should().NotBeNullOrWhiteSpace(
            "restful-booker auth should succeed");

        // Phase 2 — Inject saucedemo session cookie
        await InjectCookieAsync(
            name: "session-username",
            value: Settings.Ui.DefaultUser,
            domain: "www.saucedemo.com",
            path: "/");

        // Phase 3 — UI operations on the authenticated session
        var inventoryPage = new InventoryPage(Page);
        await inventoryPage.NavigateAsync();

        await inventoryPage.AddItemToCartByNameAsync("Sauce Labs Backpack");
        await inventoryPage.AddItemToCartByNameAsync("Sauce Labs Fleece Jacket");

        var badge = await inventoryPage.GetCartBadgeCountAsync();
        badge.Should().Be(2);

        // Navigate to cart and verify
        await inventoryPage.GoToCartAsync();
        var cartPage = new CartPage(Page);

        var cartItems = await cartPage.GetCartItemNamesAsync();
        cartItems.Should().Contain("Sauce Labs Backpack")
            .And.Contain("Sauce Labs Fleece Jacket");
    }

    /// <summary>
    /// Hybrid flow: Creates a booking via API, then validates
    /// UI state after cookie-based authentication.
    /// </summary>
    [Fact]
    public async Task HybridFlow_ApiDataRetrieval_ThenUiSortVerification()
    {
        // Phase 1 — Create and retrieve data from booker API
        using var bookerClient = CreateApiClient(Settings.Api.BookerBaseUrl);
        var booking = new Booking(
            Firstname: "Test",
            Lastname: "Hybrid",
            Totalprice: 99,
            Depositpaid: true,
            Bookingdates: new BookingDates("2026-10-01", "2026-10-05"));

        var created = await bookerClient.CreateBookingAsync(booking);
        created.BookingId.Should().BePositive("API booking creation should succeed");

        var fetched = await bookerClient.GetBookingByIdAsync(created.BookingId);
        fetched.Firstname.Should().Be("Test");

        // Phase 2 — Inject session
        await InjectCookieAsync(
            name: "session-username",
            value: Settings.Ui.DefaultUser,
            domain: "www.saucedemo.com",
            path: "/");

        // Phase 3 — Verify UI sorting after bypassed login
        var inventoryPage = new InventoryPage(Page);
        await inventoryPage.NavigateAsync();

        await inventoryPage.SortByAsync("za");

        var productNames = await inventoryPage.GetAllProductNamesAsync();
        productNames.Should().BeInDescendingOrder(
            "sorting Z-A should yield descending alphabetical order");
    }
}
