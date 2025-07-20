# AutomationExercise.Tests Framework

A C# test automation framework using **Playwright** and **NUnit** for testing https://automationexercise.com/

## ✨ Framework Highlights

- **🔧 Zero Hardcoding:** Fully configuration-driven with `appsettings.json`
- **🌍 Multi-Environment:** Easy staging/production testing with config overrides
- **🎯 34 Automated Tests** with 100% pass rate
- **📊 Rich Reporting:** HTML reports with screenshots and detailed metrics
- **🏗️ Enterprise Architecture:** Page Object Model with centralized utilities
- **🚀 CI/CD Ready:** PowerShell automation and multi-browser support

## Quick Start

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PowerShell 7+](https://github.com/PowerShell/PowerShell) (recommended)
- IDE: [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### Installation
```bash
# Clone the repository
git clone <repository-url>
cd AutomationExercise.Tests

# Restore dependencies
dotnet restore

# Install Playwright browsers
pwsh bin/Debug/net8.0/playwright.ps1 install

# Run tests to verify setup
.\RunTestsWithReport.ps1
```

## Configuration Management 🔧

### Main Configuration (`appsettings.json`)
The framework is fully configuration-driven with no hardcoded values:

```json
{
  "TestSettings": {
    "BaseUrl": "https://automationexercise.com",
    "DefaultTimeout": 30000,
    "RetryCount": 3,
    "ScreenshotPath": "screenshots",
    "ReportsPath": "reports",
    "Currency": "Rs.",
    "Locale": "en-US"
  },
  "BrowserSettings": {
    "DefaultBrowser": "chromium",
    "Headless": false,
    "SlowMotion": 0,
    "ViewportWidth": 1920,
    "ViewportHeight": 1080
  },
  "Selectors": {
    "HomePage": {
      "ProductsLink": "a[href='/products']",
      "CartLink": "a[href='/view_cart']",
      "LoginLink": "a[href='/login']"
    },
    "ProductsPage": {
      "SearchBox": "#search_product",
      "SearchButton": "#submit_search",
      "ProductContainer": ".single-products",
      "ProductName": "h2",
      "ProductPrice": "h2",
      "AddToCartButton": ".add-to-cart"
    }
  },
  "Timeouts": {
    "ElementWait": 10000,
    "PageLoad": 30000,
    "NetworkIdle": 15000,
    "ModalWait": 5000
  }
}
```

### Environment-Specific Overrides
Create `appsettings.staging.json` for different environments:
```json
{
  "TestSettings": {
    "BaseUrl": "https://staging.automationexercise.com",
    "DefaultTimeout": 45000
  },
  "BrowserSettings": {
    "Headless": true
  }
}
```

### External Test Data
Test data is externalized in `TestData/` folder:
```json
{
  "SearchTerms": [
    {
      "term": "shirt",
      "expectedCount": 5,
      "description": "Basic clothing search"
    },
    {
      "term": "dress",
      "expectedCount": 8,
      "description": "Women's clothing search"
    }
  ]
}
```

## Running Tests

### Basic Usage
```powershell
# Run all tests with HTML report
.\RunTestsWithReport.ps1

# Run specific test categories
.\RunTestsWithReport.ps1 -TestFilter "Category=Search"
.\RunTestsWithReport.ps1 -TestFilter "Category=Cart"

# Run specific tests
.\RunTestsWithReport.ps1 -TestFilter "TestName~NavigateToProducts"
```

### Browser Options
```powershell
# Different browsers (configs override defaults)
.\RunTestsWithReport.ps1 -BrowserType chromium  # Default
.\RunTestsWithReport.ps1 -BrowserType firefox
.\RunTestsWithReport.ps1 -BrowserType webkit

# Headless vs headed
.\RunTestsWithReport.ps1 -Headless $true   # For CI/CD
.\RunTestsWithReport.ps1 -Headless $false  # For debugging
```

### Environment Overrides
```powershell
# Use staging configuration
$env:ASPNETCORE_ENVIRONMENT="staging"
.\RunTestsWithReport.ps1

# Override specific settings via environment variables
$env:BaseUrl="https://custom.environment.com"
$env:DefaultTimeout="45000"
.\RunTestsWithReport.ps1
```

## Project Structure

```
AutomationExercise.Tests/
├── Configuration/               # Configuration management
│   ├── appsettings.json        # Main configuration
│   ├── appsettings.staging.json # Environment override
│   └── Configuration-README.md # Config documentation
├── Pages/                      # Page Object Model
│   ├── BasePage.cs            # Common page functionality
│   ├── ProductsPage.cs        # Products page interactions
│   └── CartPage.cs            # Cart page interactions
├── Tests/                     # Test classes
│   ├── BaseTest.cs            # Test setup and utilities
│   ├── ProductSearchTests.cs  # Search functionality tests
│   └── CartValidationTests.cs # Cart validation tests
├── Helpers/                   # Utility classes
│   ├── ConfigManager.cs       # Configuration loader
│   ├── WaitStrategies.cs      # Centralized wait logic
│   ├── TestDataFactory.cs    # Test data generation
│   └── TestConstants.cs      # Framework constants
├── TestData/                  # External test data
│   └── SearchTestData.json    # Search test parameters
├── Models/                    # Data models
├── screenshots/               # Auto-captured screenshots
├── reports/                   # HTML test reports
└── RunTestsWithReport.ps1     # Test execution script
```

## Writing Tests

### Basic Test Structure
```csharp
[TestFixture]
[Category("YourCategory")]
public class YourTests : BaseTest
{
    [Test]
    public async Task YourTest_Should_DoSomething()
    {
        // Configuration automatically loaded
        var baseUrl = ConfigManager.TestConfig.BaseUrl;
        var timeout = ConfigManager.TestConfig.DefaultTimeout;
        
        // Arrange
        await NavigateToPageAsync();
        
        // Act
        await PerformAction();
        
        // Assert
        result.Should().BeExpected();
    }
}
```

### Using Configuration-Driven Page Objects
```csharp
[Test]
public async Task SearchProducts_ShouldReturnResults()
{
    // Navigate using configured base URL
    await NavigateToProductsPageAsync();
    
    // Use selectors from configuration
    await _productsPage!.SearchProductsAsync("shirt");
    var results = await _productsPage.GetSearchResultsAsync();
    
    // Timeouts from configuration
    results.Should().NotBeEmpty();
    results.Count.Should().BeGreaterThan(0);
}
```

### External Test Data Usage
```csharp
// Test data loaded from external JSON files
[TestCaseSource(nameof(SearchTestData))]
public async Task SearchProducts_WithTerm_ShouldReturnResults(
    string searchTerm, int expectedCount, string description)
{
    // Test implementation using external data
}

private static IEnumerable<object[]> SearchTestData() => 
    TestDataFactory.GetSearchTestDataFromFile();
```

## Configuration Deep Dive

### How Configuration Works
The framework uses `Microsoft.Extensions.Configuration` for hierarchical configuration:

1. **Base Configuration:** `appsettings.json`
2. **Environment Override:** `appsettings.{Environment}.json`
3. **Environment Variables:** Override any setting
4. **Runtime Parameters:** Command-line arguments

### Accessing Configuration
```csharp
// Strongly-typed configuration access
var testConfig = ConfigManager.TestConfig;
var selectors = ConfigManager.Selectors;
var timeouts = ConfigManager.Timeouts;

// Example usage in page objects
public class ProductsPage : BasePage
{
    private readonly ProductsPageSelectors _selectors;
    
    public ProductsPage(IPage page) : base(page)
    {
        _selectors = ConfigManager.Selectors.ProductsPage;
    }
    
    public async Task SearchAsync(string term)
    {
        await FillAsync(_selectors.SearchBox, term);
        await ClickAsync(_selectors.SearchButton);
    }
}
```

### Adding New Configuration
1. Update `appsettings.json` with new settings
2. Add properties to configuration classes in `ConfigManager.cs`
3. Use strongly-typed access throughout framework

## CI/CD Integration

### GitHub Actions
```yaml
name: Test Automation

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        browser: [chromium, firefox, webkit]
        environment: [production, staging]
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
    
    - name: Install dependencies
      run: dotnet restore
    
    - name: Install Playwright
      run: pwsh bin/Debug/net8.0/playwright.ps1 install
    
    - name: Run tests
      run: dotnet test --configuration Release --logger trx
      env:
        ASPNETCORE_ENVIRONMENT: ${{ matrix.environment }}
        BrowserType: ${{ matrix.browser }}
        Headless: true
    
    - name: Upload test results
      uses: actions/upload-artifact@v4
      if: always()
      with:
        name: test-results-${{ matrix.browser }}-${{ matrix.environment }}
        path: TestResults/
```

## Troubleshooting

### Common Issues

| Problem                        | Solution                                                      |
|---------                       |----------                                                     |
| `Configuration file not found` | Ensure `appsettings.json` is copied to output directory       |
| `Browser not found`            | Run `pwsh bin/Debug/net8.0/playwright.ps1 install`            |
| `Tests timeout`                | Check `DefaultTimeout` in configuration or network connection |
| `Element not found`            | Verify selectors in `appsettings.json` match current website  |
| `Port already in use`          | Kill existing browser processes                               |

### Debug Options
```powershell
# Verbose logging with configuration details
.\RunTestsWithReport.ps1 -Verbose

# Keep browser open on failure for inspection
.\RunTestsWithReport.ps1 -Headless $false -StayOpen $true

# Use staging environment
$env:ASPNETCORE_ENVIRONMENT="staging"
.\RunTestsWithReport.ps1
```

### Configuration Validation
The framework validates configuration on startup:
```csharp
// Automatic validation catches configuration errors early
public void ValidateConfiguration()
{
    if (string.IsNullOrEmpty(BaseUrl))
        throw new ConfigurationException("BaseUrl is required");
    
    if (DefaultTimeout <= 0)
        throw new ConfigurationException("DefaultTimeout must be positive");
    
    // Additional validations...
}
```

## Framework Features

### Zero Hardcoding Architecture
- **All URLs** configurable via `appsettings.json`
- **All selectors** externalized and environment-specific
- **All timeouts** configurable with fallbacks
- **Test data** in external JSON files
- **Easy environment switching** without code changes

### Advanced Wait Strategies
```csharp
// Configuration-driven timeouts
await WaitStrategies.WaitForPageReadyAsync(_page, ConfigManager.Timeouts.PageLoad);
await WaitStrategies.WaitForElementInteractableAsync(_page, selector, ConfigManager.Timeouts.ElementWait);
```

### Automatic Screenshots with Context
Screenshots automatically capture configuration state and test parameters for debugging.

### Multi-Environment Test Data
```csharp
// Environment-specific test data
var searchData = TestDataFactory.GetSearchDataForEnvironment(
    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "production"
);
```

## Contributing

### Configuration Guidelines
- **Never hardcode values** - use configuration
- **Validate new configurations** in `ConfigManager`
- **Document configuration changes** in `Configuration-README.md`
- **Test with multiple environments** before submitting

### Adding New Features
1. **Update configuration schema** in `appsettings.json`
2. **Add strongly-typed classes** in `ConfigManager.cs`
3. **Update environment overrides** as needed
4. **Add validation logic** for new configuration
5. **Document usage** in this README

## Resources

- **[Test Summary Report](./TestSummaryReport.md)** - Executive overview and metrics
- **[Interview Preparation](./InterviewPreparation.md)** - Technical deep dive
- **[Bug Reports](./BugReports/)** - Known issues and solutions

## Dependencies

- **Microsoft.Playwright** 1.40.0 - Browser automation
- **Microsoft.Extensions.Configuration** 9.0.7 - Configuration management
- **Microsoft.Extensions.Configuration.Json** 9.0.7 - JSON configuration provider
- **Microsoft.Extensions.Configuration.EnvironmentVariables** 9.0.7 - Environment overrides
- **NUnit** 3.14.0 - Testing framework
- **FluentAssertions** 6.12.0 - Test assertions
- **ExtentReports** 5.0.1 - HTML reporting
- **Bogus** 35.4.0 - Test data generation
---

**🎯 Key Achievement:** This framework demonstrates enterprise-level configuration management with zero hardcoded values, making it highly maintainable and environment-flexible.