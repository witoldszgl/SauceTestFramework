# SauceTestFramework

A test automation framework built with C# (.NET 8), Microsoft Playwright, and HttpClient. It implements a hybrid testing pattern combining API and UI testing to optimize execution speed.

## Core Features
* **Hybrid Testing Pattern**: Uses fast REST API calls to handle authentication and state setup, bypassing slow UI login screens by injecting session cookies directly into the Playwright `BrowserContext`.
* **Page Object Model (POM)**: Strict separation of UI locators and interactions from test logic.
* **REST API Client**: A dedicated wrapper around `HttpClient` for backend data setup and validation.
* **Modern C# Tooling**: Built with xUnit, FluentAssertions, records, and primary constructors.

## Requirements
* .NET 8.0 SDK
* PowerShell (for installing Playwright dependencies)

## Setup & Execution

### 1. Installation
Clone the repository and install the required browser binaries for Playwright:
```bash
git clone https://github.com/your-org/SauceTestFramework.git
cd SauceTestFramework
dotnet restore
dotnet build
pwsh src/SauceTestFramework/bin/Debug/net8.0/playwright.ps1 install
```

### 2. Running the Tests
Execute all tests (API, UI, and Hybrid):
```bash
dotnet test --verbosity normal
```

To run a specific test category:
```bash
# Run only API tests
dotnet test --filter "FullyQualifiedName~ApiTests"

# Run only UI tests
dotnet test --filter "FullyQualifiedName~UiTests"

# Run only Hybrid tests
dotnet test --filter "FullyQualifiedName~HybridTests"
```

### 3. Configuration
Framework settings are managed in `src/SauceTestFramework/appsettings.json`.
To run UI tests with the browser visible (headed mode), set `HeadlessMode` to `false`:
```json
{
  "Ui": {
    "HeadlessMode": false,
    "SlowMotionMs": 500
  }
}
```

## Project Structure

* `src/SauceTestFramework/Config/` - Framework configuration and environment settings.
* `src/SauceTestFramework/Models/` - Data Transfer Objects (DTOs) for API payloads.
* `src/SauceTestFramework/Clients/` - HTTP clients handling REST API communication.
* `src/SauceTestFramework/Pages/` - Playwright Page Objects containing element locators.
* `src/SauceTestFramework/Fixtures/` - `BaseTest` class managing the Playwright lifecycle, browser contexts, and cookie injection.
* `src/SauceTestFramework/Tests/` - Test implementation files separated by category (`ApiTests`, `UiTests`, `HybridTests`).

## CI/CD Integration
The project includes a GitHub Actions workflow (`.github/workflows/dotnet.yml`) that automatically restores dependencies, installs Playwright browsers, runs the test suite headlessly, and publishes the test results.
