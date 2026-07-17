namespace SauceTestFramework.Config;

/// <summary>
/// Strongly-typed settings hydrated from appsettings.json.
/// </summary>
public sealed record TestSettings
{
    public UiSettings Ui { get; init; } = new();
    public ApiSettings Api { get; init; } = new();
}

public sealed record UiSettings
{
    public string BaseUrl { get; init; } = "https://www.saucedemo.com";
    public string DefaultUser { get; init; } = "standard_user";
    public string DefaultPassword { get; init; } = "secret_sauce";
    public bool HeadlessMode { get; init; } = true;
    public int SlowMotionMs { get; init; }
    public int DefaultTimeoutMs { get; init; } = 30_000;
}

public sealed record ApiSettings
{
    public string ReqresBaseUrl { get; init; } = "https://reqres.in/api";
    public string BookerBaseUrl { get; init; } = "https://restful-booker.herokuapp.com";
    public string BookerUsername { get; init; } = "admin";
    public string BookerPassword { get; init; } = "password123";
}
