using Microsoft.Playwright;

namespace SauceTestFramework.Pages;

/// <summary>
/// Base class for all Page Objects. Provides common navigation and
/// element interaction helpers backed by Playwright.
/// </summary>
public abstract class BasePage(IPage page)
{
    protected IPage Page { get; } = page;

    /// <summary>
    /// Navigates to the page-specific URL defined in derived classes.
    /// </summary>
    public async Task NavigateAsync()
    {
        await Page.GotoAsync(PageUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// The URL (relative or absolute) that this page represents.
    /// </summary>
    protected abstract string PageUrl { get; }

    // ──────────────────────── Common Helpers ────────────────────────

    protected async Task ClickAsync(string selector) =>
        await Page.Locator(selector).ClickAsync();

    protected async Task FillAsync(string selector, string value) =>
        await Page.Locator(selector).FillAsync(value);

    protected async Task<string> GetTextAsync(string selector) =>
        await Page.Locator(selector).InnerTextAsync();

    protected async Task<bool> IsVisibleAsync(string selector) =>
        await Page.Locator(selector).IsVisibleAsync();

    protected async Task<int> CountAsync(string selector) =>
        await Page.Locator(selector).CountAsync();

    protected ILocator Locator(string selector) =>
        Page.Locator(selector);
}
