using Microsoft.Playwright;
using AutomationExercise.Tests.Pages;
using AutomationExercise.Tests.Helpers;
using AventStack.ExtentReports;
using NUnit.Framework;

namespace AutomationExercise.Tests.Tests;

/// <summary>
/// Base test class providing common setup and teardown for Playwright tests
/// </summary>
[TestFixture]
public abstract class BaseTest
{
    protected IPlaywright? _playwright;
    protected IBrowser? _browser;
    protected IPage? _page;
    protected IBrowserContext? _context;
    protected ExtentTest? _extentTest;

    // Page Objects
    protected HomePage? _homePage;
    protected ProductsPage? _productsPage;
    protected CartPage? _cartPage;

    [OneTimeSetUp]
    public async Task GlobalSetUp()
    {
        // Playwright installation is now handled by GlobalTestSetup
        await Task.CompletedTask;
    }

    [SetUp]
    public async Task SetUp()
    {
        // Create test in report
        var testName = TestContext.CurrentContext.Test.Name;
        var testDescription = TestContext.CurrentContext.Test.Properties.Get("Description")?.ToString() ?? "";
        _extentTest = ReportManager.CreateTest(testName, testDescription);
        
        ReportManager.LogInfo(_extentTest, $"Starting test: {testName}");

        // Get test configuration
        var config = GetTestConfiguration();
        
        ReportManager.LogInfo(_extentTest, $"Browser: {config.Browser}, Headless: {config.Headless}, Timeout: {config.Timeout}ms");
        
        // Add test category to report
        var categories = TestContext.CurrentContext.Test.Properties["Category"];
        if (categories != null)
        {
            foreach (var category in categories)
            {
                _extentTest.AssignCategory(category.ToString());
            }
        }

        // Initialize Playwright
        _playwright = await Playwright.CreateAsync();

        // Launch browser based on configuration
        _browser = config.Browser.ToLower() switch
        {
            "firefox" => await _playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = config.Headless,
                SlowMo = config.SlowMotion
            }),
            "webkit" => await _playwright.Webkit.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = config.Headless,
                SlowMo = config.SlowMotion
            }),
            // Default to Chromium
            _ => await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = config.Headless,
                SlowMo = config.SlowMotion,
                Args = new[] { "--disable-web-security", "--disable-features=VizDisplayCompositor" }
            })
        };

        // Create browser context with viewport and other options
        _context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
            IgnoreHTTPSErrors = true,
            AcceptDownloads = true
        });

        // Set the default timeout for actions like clicks and waits
        _context.SetDefaultTimeout(config.Timeout);

        // Enable request/response logging if needed
        if (TestContext.Parameters.Get("logging", "false").Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            _context.Request += (_, e) => TestContext.WriteLine($"Request: {e.Method} {e.Url}");
            _context.Response += (_, e) => TestContext.WriteLine($"Response: {e.Status} {e.Url}");
        }

        // Create page
        _page = await _context.NewPageAsync();

        // Initialize Page Objects
        InitializePageObjects();

        // Create screenshots directory
        Directory.CreateDirectory("screenshots");
    }

    [TearDown]
    public async Task TearDown()
    {
        var testResult = TestContext.CurrentContext.Result.Outcome.Status;
        var testName = TestContext.CurrentContext.Test.Name;
        
        // Handle test result in report
        if (_extentTest != null)
        {
            switch (testResult)
            {
                case NUnit.Framework.Interfaces.TestStatus.Passed:
                    ReportManager.LogPass(_extentTest, "Test completed successfully");
                    break;
                case NUnit.Framework.Interfaces.TestStatus.Failed:
                    var errorMessage = TestContext.CurrentContext.Result.Message ?? "Test failed";
                    var stackTrace = TestContext.CurrentContext.Result.StackTrace;
                    ReportManager.LogFail(_extentTest, errorMessage);
                    if (!string.IsNullOrEmpty(stackTrace))
                    {
                        _extentTest.Info($"<pre>{stackTrace}</pre>");
                    }
                    break;
                case NUnit.Framework.Interfaces.TestStatus.Skipped:
                    ReportManager.LogSkip(_extentTest, TestContext.CurrentContext.Result.Message ?? "Test skipped");
                    break;
            }
        }
        
        // Take screenshot on failure
        if (testResult == NUnit.Framework.Interfaces.TestStatus.Failed)
        {
            // Log page state before taking screenshot
            await LogPageStateAsync("Test Failed");
            
            var sanitizedTestName = string.Join("_", testName.Split(Path.GetInvalidFileNameChars()));
            await TakeScreenshotWithDetailsAsync($"failed_{sanitizedTestName}");
        }

        // Clean up resources
        await _page?.CloseAsync()!;
        await _context?.CloseAsync()!;
        await _browser?.CloseAsync()!;
        _playwright?.Dispose();
    }
    
    [OneTimeTearDown]
    public async Task GlobalTearDown()
    {
        // Report flush is now handled by GlobalTestSetup
        await Task.CompletedTask;
    }

    /// <summary>
    /// Initialize page objects
    /// </summary>
    private void InitializePageObjects()
    {
        if (_page != null)
        {
            _homePage = new HomePage(_page);
            _productsPage = new ProductsPage(_page);
            _cartPage = new CartPage(_page);
        }
    }

    /// <summary>
    /// Take a screenshot for debugging
    /// </summary>
    protected async Task<string> TakeScreenshotAsync(string name)
    {
        if (_page != null)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = $"screenshots/{name}_{timestamp}.png";
            await _page.ScreenshotAsync(new PageScreenshotOptions 
            { 
                Path = filename,
                FullPage = true 
            });
            TestContext.WriteLine($"Screenshot saved: {filename}");
            return Path.GetFullPath(filename);
        }
        return string.Empty;
    }

    /// <summary>
    /// Wait for a specified amount of time
    /// </summary>
    protected async Task WaitAsync(int milliseconds)
    {
        await Task.Delay(milliseconds);
    }

    /// <summary>
    /// Navigate to home page and ensure it's loaded
    /// </summary>
    protected async Task NavigateToHomePageAsync()
    {
        await _homePage!.NavigateAsync();
        var isLoaded = await _homePage.IsPageLoadedAsync();
        isLoaded.Should().BeTrue("Home page should be loaded");
    }

    /// <summary>
    /// Navigate to products page and ensure it's loaded
    /// </summary>
    protected async Task NavigateToProductsPageAsync()
    {
        await _productsPage!.NavigateAsync();
        var isLoaded = await _productsPage.IsPageLoadedAsync();
        isLoaded.Should().BeTrue("Products page should be loaded");
    }

    /// <summary>
    /// Navigate to cart page and ensure it's loaded
    /// </summary>
    protected async Task NavigateToCartPageAsync()
    {
        await _cartPage!.NavigateAsync();
        var isLoaded = await _cartPage.IsPageLoadedAsync();
        isLoaded.Should().BeTrue("Cart page should be loaded");
    }

    /// <summary>
    /// Assert that a condition is met with retry mechanism
    /// </summary>
    protected async Task AssertWithRetryAsync(Func<Task<bool>> condition, string message, int maxRetries = 3, int delayMs = 1000)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            if (await condition())
            {
                return;
            }
            
            if (i < maxRetries - 1)
            {
                await Task.Delay(delayMs);
            }
        }
        
        Assert.Fail(message);
    }

    /// <summary>
    /// Get test run configuration from environment or parameters
    /// </summary>
    protected TestConfiguration GetTestConfiguration()
    {
        // Try TestContext parameters first, then environment variables, then defaults
        var browser = TestContext.Parameters.Get("browser") ?? 
                     Environment.GetEnvironmentVariable("TEST_BROWSER") ?? 
                     "chromium";
                     
        var headless = TestContext.Parameters.Get("headless") ?? 
                      Environment.GetEnvironmentVariable("TEST_HEADLESS") ?? 
                      "false";
                      
        var timeout = TestContext.Parameters.Get("timeout") ?? 
                     Environment.GetEnvironmentVariable("TEST_TIMEOUT") ?? 
                     ConfigManager.TestConfig.Timeouts.Default.ToString();
                     
        var slowmo = TestContext.Parameters.Get("slowmo") ?? 
                    Environment.GetEnvironmentVariable("TEST_SLOWMO") ?? 
                    "0";
        
        return new TestConfiguration
        {
            BaseUrl = TestContext.Parameters.Get("baseUrl", ConfigManager.TestConfig.BaseUrl),
            Browser = browser,
            Headless = headless.Equals("true", StringComparison.OrdinalIgnoreCase),
            Timeout = int.Parse(timeout),
            SlowMotion = int.Parse(slowmo)
        };
    }

    /// <summary>
    /// Log test information
    /// </summary>
    protected void LogTestInfo(string message)
    {
        TestContext.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
        
        // Also log to ExtentReports if available
        if (_extentTest != null)
        {
            ReportManager.LogInfo(_extentTest, message);
        }
    }
    
    /// <summary>
    /// Log test step pass
    /// </summary>
    protected void LogTestPass(string message)
    {
        TestContext.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✅ {message}");
        
        if (_extentTest != null)
        {
            ReportManager.LogPass(_extentTest, message);
        }
    }
    
    /// <summary>
    /// Log test step warning
    /// </summary>
    protected void LogTestWarning(string message)
    {
        TestContext.WriteLine($"[{DateTime.Now:HH:mm:ss}] ⚠️ {message}");
        
        if (_extentTest != null)
        {
            ReportManager.LogWarning(_extentTest, message);
        }
    }
    
    /// <summary>
    /// Log current page state for debugging
    /// </summary>
    protected async Task LogPageStateAsync(string context = "")
    {
        if (_page == null || _extentTest == null) return;
        
        try
        {
            var currentUrl = _page.Url;
            var pageTitle = await _page.TitleAsync();
            var viewportSize = _page.ViewportSize;
            
            var stateInfo = $"Page State{(string.IsNullOrEmpty(context) ? "" : $" - {context}")}:\n" +
                           $"  URL: {currentUrl}\n" +
                           $"  Title: {pageTitle}\n" +
                           $"  Viewport: {viewportSize?.Width}x{viewportSize?.Height}";
            
            ReportManager.LogInfo(_extentTest, stateInfo);
            
            // Log to console as well
            TestContext.WriteLine($"[{DateTime.Now:HH:mm:ss}] 📍 {stateInfo}");
        }
        catch (Exception ex)
        {
            LogTestWarning($"Could not log page state: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Take a screenshot and add to report with current page info
    /// </summary>
    protected async Task<string> TakeScreenshotWithDetailsAsync(string name)
    {
        var screenshotPath = await TakeScreenshotAsync(name);
        
        if (!string.IsNullOrEmpty(screenshotPath) && _extentTest != null && _page != null)
        {
            try
            {
                var currentUrl = _page.Url;
                var description = $"Screenshot: {name} | URL: {currentUrl}";
                ReportManager.AddScreenshot(_extentTest, screenshotPath, description);
            }
            catch
            {
                // Ignore errors in enhanced logging
            }
        }
        
        return screenshotPath;
    }
}

/// <summary>
/// Test configuration class
/// </summary>
public class TestConfiguration
{
    public string BaseUrl { get; set; } = "https://automationexercise.com";
    public string Browser { get; set; } = "chromium";
    public bool Headless { get; set; } = false;
    public int Timeout { get; set; } = 30000;
    public int SlowMotion { get; set; } = 0;
} 