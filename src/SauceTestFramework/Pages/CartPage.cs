using Microsoft.Playwright;

namespace SauceTestFramework.Pages;

/// <summary>
/// Page Object for the saucedemo.com Cart page.
/// </summary>
public sealed class CartPage(IPage page) : BasePage(page)
{
    protected override string PageUrl => "https://www.saucedemo.com/cart.html";

    // ── Selectors ──
    private const string CartItems = "[data-test='inventory-item']";
    private const string CartItemName = "[data-test='inventory-item-name']";
    private const string ContinueShoppingButton = "[data-test='continue-shopping']";
    private const string CheckoutButton = "[data-test='checkout']";

    // ── Queries ──

    public async Task<int> GetCartItemCountAsync() =>
        await CountAsync(CartItems);

    public async Task<List<string>> GetCartItemNamesAsync()
    {
        var elements = Page.Locator(CartItemName);
        var count = await elements.CountAsync();
        var names = new List<string>(count);
        for (var i = 0; i < count; i++)
            names.Add(await elements.Nth(i).InnerTextAsync());
        return names;
    }

    public Task<bool> IsOnCartPageAsync() =>
        Task.FromResult(Page.Url.Contains("cart.html"));

    // ── Actions ──

    public async Task ContinueShoppingAsync() =>
        await ClickAsync(ContinueShoppingButton);

    public async Task ProceedToCheckoutAsync() =>
        await ClickAsync(CheckoutButton);
}
