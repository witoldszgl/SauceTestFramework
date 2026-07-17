using System.Text.Json;
using SauceTestFramework.Clients;
using SauceTestFramework.Config;
using Microsoft.Playwright;

namespace SauceTestFramework.Fixtures;

/// <summary>
/// Base test class that manages Playwright lifecycle (browser, context, page).
/// All test classes should inherit from this to get a fresh browser context per test.
/// Implements IAsyncLifetime for xUnit async setup/teardown.
/// </summary>
public abstract class BaseTest : IAsyncLifetime
{
    // ── Playwright infrastructure ──
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;

    /// <summary>The isolated browser context for the current test.</summary>
    protected IBrowserContext Context { get; private set; } = null!;

    /// <summary>The active page within the browser context.</summary>
    protected IPage Page { get; private set; } = null!;

    /// <summary>Strongly-typed test settings loaded from appsettings.json.</summary>
    protected TestSettings Settings { get; private set; } = null!;

    // ── Lifecycle ──

    public async Task InitializeAsync()
    {
        Settings = LoadSettings();

        _playwright = await Playwright.CreateAsync();

        var isCi = Environment.GetEnvironmentVariable("CI") == "true";
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = isCi || Settings.Ui.HeadlessMode,
            SlowMo = Settings.Ui.SlowMotionMs
        });

        Context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
            IgnoreHTTPSErrors = true
        });

        Context.SetDefaultTimeout(Settings.Ui.DefaultTimeoutMs);

        Page = await Context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await Page.CloseAsync();
        await Context.CloseAsync();
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    // ── Helpers available to all tests ──

    /// <summary>
    /// Creates an <see cref="ApiClient"/> pre-configured with the given base URL.
    /// </summary>
    protected ApiClient CreateApiClient(string baseUrl) => new(baseUrl);

    /// <summary>
    /// Injects a cookie into the current browser context.
    /// This is the core mechanism enabling the hybrid API → UI flow.
    /// </summary>
    protected async Task InjectCookieAsync(
        string name,
        string value,
        string domain,
        string path = "/",
        bool secure = false,
        bool httpOnly = false)
    {
        await Context.AddCookiesAsync(new[]
        {
            new Cookie
            {
                Name = name,
                Value = value,
                Domain = domain,
                Path = path,
                Secure = secure,
                HttpOnly = httpOnly
            }
        });
    }

    /// <summary>
    /// Injects a value into the browser's localStorage for the given origin.
    /// </summary>
    protected async Task InjectLocalStorageAsync(string url, string key, string value)
    {
        await Page.GotoAsync(url);
        await Page.EvaluateAsync($"window.localStorage.setItem('{key}', '{value}')");
    }

    // ── Private ──

    private static TestSettings LoadSettings()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path))
            return new TestSettings();

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<TestSettings>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new TestSettings();
    }
}
