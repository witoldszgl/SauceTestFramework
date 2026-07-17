using Microsoft.Playwright;

namespace SauceTestFramework.Pages;

/// <summary>
/// Page Object for the saucedemo.com Login page.
/// </summary>
public sealed class LoginPage(IPage page) : BasePage(page)
{
    protected override string PageUrl => "https://www.saucedemo.com";

    // ── Selectors ──
    private const string UsernameInput = "#user-name";
    private const string PasswordInput = "#password";
    private const string LoginButton = "#login-button";
    private const string ErrorMessage = "[data-test='error']";

    // ── Actions ──

    public async Task LoginAsync(string username, string password)
    {
        await FillAsync(UsernameInput, username);
        await FillAsync(PasswordInput, password);
        await ClickAsync(LoginButton);
    }

    public async Task<string> GetErrorMessageAsync() =>
        await GetTextAsync(ErrorMessage);

    public async Task<bool> IsErrorVisibleAsync() =>
        await IsVisibleAsync(ErrorMessage);

    public async Task<bool> IsOnLoginPageAsync() =>
        await IsVisibleAsync(LoginButton);
}
