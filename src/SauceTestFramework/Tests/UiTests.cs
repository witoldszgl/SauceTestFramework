using FluentAssertions;
using SauceTestFramework.Fixtures;
using SauceTestFramework.Pages;

namespace SauceTestFramework.Tests;

/// <summary>
/// Pure UI tests against saucedemo.com using the Page Object Model.
/// Each test gets a fresh browser context via <see cref="BaseTest"/>.
/// </summary>
public sealed class UiTests : BaseTest
{
    [Fact]
    public async Task Login_WithValidCredentials_ShouldNavigateToInventory()
    {
        // Arrange
        var loginPage = new LoginPage(Page);
        await loginPage.NavigateAsync();

        // Act
        await loginPage.LoginAsync(
            Settings.Ui.DefaultUser,
            Settings.Ui.DefaultPassword);

        // Assert
        var inventoryPage = new InventoryPage(Page);
        (await inventoryPage.IsOnInventoryPageAsync()).Should().BeTrue(
            "valid credentials should land the user on the inventory page");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldShowError()
    {
        // Arrange
        var loginPage = new LoginPage(Page);
        await loginPage.NavigateAsync();

        // Act
        await loginPage.LoginAsync("invalid_user", "wrong_password");

        // Assert
        (await loginPage.IsErrorVisibleAsync()).Should().BeTrue();
        var errorText = await loginPage.GetErrorMessageAsync();
        errorText.Should().Contain("Username and password do not match");
    }

    [Fact]
    public async Task Inventory_ShouldDisplaySixProducts()
    {
        // Arrange — login first
        var loginPage = new LoginPage(Page);
        await loginPage.NavigateAsync();
        await loginPage.LoginAsync(
            Settings.Ui.DefaultUser,
            Settings.Ui.DefaultPassword);

        var inventoryPage = new InventoryPage(Page);

        // Act
        var itemCount = await inventoryPage.GetInventoryItemCountAsync();

        // Assert
        itemCount.Should().Be(6, "saucedemo always lists 6 products");
    }

    [Fact]
    public async Task AddToCart_ShouldUpdateBadgeCount()
    {
        // Arrange
        var loginPage = new LoginPage(Page);
        await loginPage.NavigateAsync();
        await loginPage.LoginAsync(
            Settings.Ui.DefaultUser,
            Settings.Ui.DefaultPassword);

        var inventoryPage = new InventoryPage(Page);

        // Act
        await inventoryPage.AddItemToCartByNameAsync("Sauce Labs Backpack");
        await inventoryPage.AddItemToCartByNameAsync("Sauce Labs Bike Light");

        // Assert
        var badge = await inventoryPage.GetCartBadgeCountAsync();
        badge.Should().Be(2);
    }

    [Fact]
    public async Task Cart_ShouldContainAddedItems()
    {
        // Arrange
        var loginPage = new LoginPage(Page);
        await loginPage.NavigateAsync();
        await loginPage.LoginAsync(
            Settings.Ui.DefaultUser,
            Settings.Ui.DefaultPassword);

        var inventoryPage = new InventoryPage(Page);
        await inventoryPage.AddItemToCartByNameAsync("Sauce Labs Backpack");
        await inventoryPage.AddItemToCartByNameAsync("Sauce Labs Onesie");

        // Act
        await inventoryPage.GoToCartAsync();

        var cartPage = new CartPage(Page);

        // Assert
        var cartItems = await cartPage.GetCartItemNamesAsync();
        cartItems.Should().HaveCount(2)
            .And.Contain("Sauce Labs Backpack")
            .And.Contain("Sauce Labs Onesie");
    }

    [Fact]
    public async Task SortProducts_ByPriceLowToHigh_ShouldReorderList()
    {
        // Arrange
        var loginPage = new LoginPage(Page);
        await loginPage.NavigateAsync();
        await loginPage.LoginAsync(
            Settings.Ui.DefaultUser,
            Settings.Ui.DefaultPassword);

        var inventoryPage = new InventoryPage(Page);

        // Act
        await inventoryPage.SortByAsync("lohi");

        // Assert
        var prices = await inventoryPage.GetAllProductPricesAsync();
        var numericPrices = prices
            .Select(p => decimal.Parse(p.Replace("$", "")))
            .ToList();

        numericPrices.Should().BeInAscendingOrder(
            "sorting by 'Price (low to high)' should produce ascending prices");
    }
}
