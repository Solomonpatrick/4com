using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace AutomationExercise.Tests.Helpers;

/// <summary>
/// Manages configuration settings for the test framework
/// </summary>
public static class ConfigManager
{
    private static readonly IConfiguration Configuration;
    private static TestConfiguration? _testConfig;
    private static SelectorsConfiguration? _selectors;
    
    static ConfigManager()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("TEST_ENV") ?? "local"}.json", optional: true)
            .AddEnvironmentVariables();
            
        Configuration = builder.Build();
        
        // Load configurations
        _testConfig = Configuration.GetSection("TestSettings").Get<TestConfiguration>();
        _selectors = Configuration.GetSection("Selectors").Get<SelectorsConfiguration>();
    }
    
    /// <summary>
    /// Get test configuration
    /// </summary>
    public static TestConfiguration TestConfig => _testConfig ?? throw new InvalidOperationException("Test configuration not loaded");
    
    /// <summary>
    /// Get selectors configuration
    /// </summary>
    public static SelectorsConfiguration Selectors => _selectors ?? throw new InvalidOperationException("Selectors configuration not loaded");
    
    /// <summary>
    /// Get a configuration value by key
    /// </summary>
    public static string GetSetting(string key)
    {
        return Configuration[key] ?? throw new InvalidOperationException($"Configuration key '{key}' not found");
    }
    
    /// <summary>
    /// Get a configuration value with a default fallback
    /// </summary>
    public static string GetSetting(string key, string defaultValue)
    {
        return Configuration[key] ?? defaultValue;
    }
    
    /// <summary>
    /// Get a configuration section
    /// </summary>
    public static IConfigurationSection GetSection(string sectionName)
    {
        return Configuration.GetSection(sectionName);
    }
}

/// <summary>
/// Test configuration settings
/// </summary>
public class TestConfiguration
{
    public string BaseUrl { get; set; } = "";
    public TimeoutSettings Timeouts { get; set; } = new();
    public RetrySettings Retry { get; set; } = new();
    public LocalizationSettings Localization { get; set; } = new();
    public PathSettings Paths { get; set; } = new();
}

public class TimeoutSettings
{
    public int Default { get; set; } = 30000;
    public int Short { get; set; } = 5000;
    public int Medium { get; set; } = 15000;
    public int Long { get; set; } = 60000;
    public int NetworkIdle { get; set; } = 10000;
    public int ElementWait { get; set; } = 5000;
}

public class RetrySettings
{
    public int MaxAttempts { get; set; } = 3;
    public int DelayMs { get; set; } = 1000;
    public int BackoffMultiplier { get; set; } = 2;
}

public class LocalizationSettings
{
    public string Currency { get; set; } = "Rs.";
    public string Locale { get; set; } = "en-US";
}

public class PathSettings
{
    public string Screenshots { get; set; } = "screenshots";
    public string Reports { get; set; } = "reports";
    public string TestData { get; set; } = "TestData";
}

/// <summary>
/// Selectors configuration
/// </summary>
public class SelectorsConfiguration
{
    public CommonSelectors Common { get; set; } = new();
    public HomePageSelectors HomePage { get; set; } = new();
    public ProductsPageSelectors ProductsPage { get; set; } = new();
    public CartPageSelectors CartPage { get; set; } = new();
}

public class CommonSelectors
{
    public CookieConsentSelectors CookieConsent { get; set; } = new();
}

public class CookieConsentSelectors
{
    public string Container { get; set; } = "";
    public string AcceptButton { get; set; } = "";
}

public class HomePageSelectors
{
    public NavigationSelectors Navigation { get; set; } = new();
    public SectionSelectors Sections { get; set; } = new();
    public ProductSelectors Products { get; set; } = new();
    public CartModalSelectors CartModal { get; set; } = new();
}

public class NavigationSelectors
{
    public string ProductsLink { get; set; } = "";
    public string CartLink { get; set; } = "";
    public string HomeLink { get; set; } = "";
    public string SignupLoginLink { get; set; } = "";
    public string ContactUsLink { get; set; } = "";
    public string TestCasesLink { get; set; } = "";
}

public class SectionSelectors
{
    public string FeaturesItems { get; set; } = "";
    public string CategorySection { get; set; } = "";
    public string BrandsSection { get; set; } = "";
}

public class ProductSelectors
{
    public string ProductItems { get; set; } = "";
    public string AddToCartButtons { get; set; } = "";
    public string ViewProductButtons { get; set; } = "";
}

public class CartModalSelectors
{
    public string Container { get; set; } = "";
    public string ViewCartButton { get; set; } = "";
    public string ContinueShoppingButton { get; set; } = "";
    public string CloseButton { get; set; } = "";
}

public class ProductsPageSelectors
{
    public SearchSelectors Search { get; set; } = new();
    public ProductGridSelectors ProductGrid { get; set; } = new();
    public CategorySelectors Categories { get; set; } = new();
    public BrandSelectors Brands { get; set; } = new();
}

public class SearchSelectors
{
    public string SearchInput { get; set; } = "";
    public string SearchButton { get; set; } = "";
}

public class ProductGridSelectors
{
    public string AllProductsTitle { get; set; } = "";
    public string ProductItems { get; set; } = "";
    public string ProductNames { get; set; } = "";
    public string ProductPrices { get; set; } = "";
    public string AddToCartButtons { get; set; } = "";
    public string ViewProductButtons { get; set; } = "";
}

public class CategorySelectors
{
    public string CategorySection { get; set; } = "";
    public string WomenCategory { get; set; } = "";
    public string MenCategory { get; set; } = "";
    public string KidsCategory { get; set; } = "";
    public string CategoryItems { get; set; } = "";
}

public class BrandSelectors
{
    public string BrandsSection { get; set; } = "";
    public string BrandItems { get; set; } = "";
}

public class CartPageSelectors
{
    public string Container { get; set; } = "";
    public string EmptyCartMessage { get; set; } = "";
    public string CartItems { get; set; } = "";
    public string ProductNames { get; set; } = "";
    public string ProductPrices { get; set; } = "";
    public string QuantityInputs { get; set; } = "";
    public string TotalPrices { get; set; } = "";
    public string RemoveButtons { get; set; } = "";
    public string ProceedToCheckout { get; set; } = "";
    public string ContinueShopping { get; set; } = "";
} 