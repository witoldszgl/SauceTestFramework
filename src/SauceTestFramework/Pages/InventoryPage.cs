using Microsoft.Playwright;

namespace SauceTestFramework.Pages;

/// <summary>
/// Page Object for the saucedemo.com Inventory (Products) page.
/// Encapsulates all product-listing interactions and verifications.
/// </summary>
public sealed class InventoryPage(IPage page) : BasePage(page)
{
    protected override string PageUrl => "https://www.saucedemo.com/inventory.html";

    // ── Selectors ──
    private const string PageTitle = "[data-test='title']";
    private const string InventoryItems = "[data-test='inventory-item']";
    private const string InventoryItemName = "[data-test='inventory-item-name']";
    private const string InventoryItemPrice = "[data-test='inventory-item-price']";
    private const string ShoppingCartBadge = "[data-test='shopping-cart-badge']";
    private const string ShoppingCartLink = "[data-test='shopping-cart-link']";
    private const string SortDropdown = "[data-test='product-sort-container']";
    private const string BurgerMenuButton = "#react-burger-menu-btn";

    // ── Queries ──

    public async Task<string> GetPageTitleAsync() =>
        await GetTextAsync(PageTitle);

    public async Task<int> GetInventoryItemCountAsync() =>
        await CountAsync(InventoryItems);

    public async Task<List<string>> GetAllProductNamesAsync()
    {
        var elements = Page.Locator(InventoryItemName);
        var count = await elements.CountAsync();
        var names = new List<string>(count);
        for (var i = 0; i < count; i++)
            names.Add(await elements.Nth(i).InnerTextAsync());
        return names;
    }

    public async Task<List<string>> GetAllProductPricesAsync()
    {
        var elements = Page.Locator(InventoryItemPrice);
        var count = await elements.CountAsync();
        var prices = new List<string>(count);
        for (var i = 0; i < count; i++)
            prices.Add(await elements.Nth(i).InnerTextAsync());
        return prices;
    }

    public async Task<bool> IsOnInventoryPageAsync() =>
        Page.Url.Contains("inventory.html") && await CountAsync(InventoryItems) > 0;

    // ── Actions ──

    /// <summary>
    /// Adds the product at the given 0-based index to the cart.
    /// </summary>
    public async Task AddItemToCartByIndexAsync(int index)
    {
        var addButton = Page.Locator(InventoryItems).Nth(index)
            .Locator("button:has-text('Add to cart')");
        await addButton.ClickAsync();
    }

    /// <summary>
    /// Adds a product by its visible name.
    /// </summary>
    public async Task AddItemToCartByNameAsync(string productName)
    {
        var item = Page.Locator(InventoryItems)
            .Filter(new() { HasText = productName });
        await item.Locator("button:has-text('Add to cart')").ClickAsync();
    }

    /// <summary>
    /// Removes a product by its visible name.
    /// </summary>
    public async Task RemoveItemFromCartByNameAsync(string productName)
    {
        var item = Page.Locator(InventoryItems)
            .Filter(new() { HasText = productName });
        await item.Locator("button:has-text('Remove')").ClickAsync();
    }

    public async Task<int?> GetCartBadgeCountAsync()
    {
        if (!await IsVisibleAsync(ShoppingCartBadge))
            return null;

        var text = await GetTextAsync(ShoppingCartBadge);
        return int.TryParse(text, out var count) ? count : null;
    }

    public async Task GoToCartAsync() =>
        await ClickAsync(ShoppingCartLink);

    public async Task SortByAsync(string optionValue) =>
        await Page.Locator(SortDropdown).SelectOptionAsync(optionValue);

    public async Task OpenBurgerMenuAsync() =>
        await ClickAsync(BurgerMenuButton);
}
