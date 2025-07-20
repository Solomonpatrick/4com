using AutomationExercise.Tests.Models;

namespace AutomationExercise.Tests.Helpers
{
    /// <summary>
    /// Test Data Factory implementing Factory and Builder patterns
    /// Provides centralized, reusable test data generation following DRY principle
    /// </summary>
    public static class TestDataFactory
    {
        #region Search Test Data

        /// <summary>
        /// Get search test data with various categories and expected results
        /// </summary>
        public static IEnumerable<object[]> GetSearchTestData()
        {
            yield return new object[] { "Top", 14, "Clothing tops" };
            yield return new object[] { "Dress", 9, "Women's dresses" };
            yield return new object[] { "Shirt", 13, "Casual and formal shirts" };
            yield return new object[] { "Jeans", 3, "Denim pants" };
            yield return new object[] { "Tshirt", 6, "T-shirts and casual wear" };
            yield return new object[] { "Saree", 3, "Traditional Indian wear" };
            yield return new object[] { "Kids", 1, "Children's clothing" };
            yield return new object[] { "Women", 2, "Women's category" };
            yield return new object[] { "Men", 4, "Men's category" };
            yield return new object[] { "Blue", 7, "Blue colored items" };
            yield return new object[] { "Pink", 6, "Pink colored items" };
            yield return new object[] { "Cotton", 6, "Cotton material items" };
            yield return new object[] { "Polo", 1, "Polo shirts" };
        }

        /// <summary>
        /// Get category filter test data
        /// </summary>
        public static IEnumerable<object[]> GetCategoryTestData()
        {
            yield return new object[] { "Women" };
            yield return new object[] { "Men" };
            yield return new object[] { "Kids" };
        }

        /// <summary>
        /// Get invalid search terms for negative testing
        /// </summary>
        public static IEnumerable<object[]> GetInvalidSearchData()
        {
            yield return new object[] { "", "Empty search term" };
            yield return new object[] { "   ", "Whitespace only" };
            yield return new object[] { "@#$%", "Special characters" };
            yield return new object[] { "123", "Numeric only" };
            yield return new object[] { "!@#$%^&*()", "All special characters" };
            yield return new object[] { "NonExistentProduct12345", "Non-existent product" };
        }
        #endregion

        #region Product Test Data

        /// <summary>
        /// Create sample product for testing
        /// </summary>
        public static Product CreateSampleProduct() => new ProductBuilder()
            .WithName("Blue Top")
            .WithPrice($"{ConfigManager.TestConfig.Localization.Currency} 500")
            .WithCategory("Women > Tops")
            .WithBrand("Polo")
            .Build();

        /// <summary>
        /// Create multiple sample products for cart testing
        /// </summary>
        public static List<Product> CreateSampleProducts(int count = 3)
        {
            var products = new List<Product>();
            var builders = new[]
            {
                new ProductBuilder().WithName("Blue Top").WithPrice("Rs. 500").WithCategory("Women > Tops"),
                new ProductBuilder().WithName("Men Tshirt").WithPrice("Rs. 400").WithCategory("Men > Tshirts"),
                new ProductBuilder().WithName("Sleeveless Dress").WithPrice("Rs. 1000").WithCategory("Women > Dress"),
                new ProductBuilder().WithName("Cotton Polo").WithPrice("Rs. 600").WithCategory("Men > Tshirts"),
                new ProductBuilder().WithName("Summer Top").WithPrice("Rs. 300").WithCategory("Women > Tops")
            };

            for (int i = 0; i < Math.Min(count, builders.Length); i++)
            {
                products.Add(builders[i].Build());
            }

            return products;
        }
        #endregion

        #region Performance Test Data

        /// <summary>
        /// Get performance test scenarios with expected thresholds
        /// </summary>
        public static IEnumerable<object[]> GetPerformanceTestData()
        {
            yield return new object[] { 5, 10000, "Cart operations", "Add/remove multiple items" };
            yield return new object[] { 10, 15000, "Search operations", "Multiple search queries" };
            yield return new object[] { 3, 5000, "Navigation", "Page navigation speed" };
        }

        /// <summary>
        /// Get load test data for stress testing
        /// </summary>
        public static IEnumerable<object[]> GetLoadTestData()
        {
            yield return new object[] { 50, "Light load" };
            yield return new object[] { 100, "Medium load" };
            yield return new object[] { 200, "Heavy load" };
        }
        #endregion

        #region Configuration Test Data

        /// <summary>
        /// Get browser configuration test data
        /// </summary>
        public static IEnumerable<object[]> GetBrowserTestData()
        {
            yield return new object[] { "chromium", false, 30000, "Standard Chrome testing" };
            yield return new object[] { "firefox", false, 35000, "Firefox compatibility" };
            yield return new object[] { "webkit", false, 40000, "Safari compatibility" };
            yield return new object[] { "chromium", true, 25000, "Headless performance" };
        }

        /// <summary>
        /// Get viewport test data for responsive testing
        /// </summary>
        public static IEnumerable<object[]> GetViewportTestData()
        {
            yield return new object[] { 1920, 1080, "Desktop Full HD" };
            yield return new object[] { 1366, 768, "Desktop Standard" };
            yield return new object[] { 768, 1024, "Tablet Portrait" };
            yield return new object[] { 1024, 768, "Tablet Landscape" };
            yield return new object[] { 375, 667, "Mobile iPhone SE" };
            yield return new object[] { 414, 896, "Mobile iPhone 11" };
        }
        #endregion

        #region Edge Case Test Data

        /// <summary>
        /// Get boundary value test data
        /// </summary>
        public static IEnumerable<object[]> GetBoundaryTestData()
        {
            yield return new object[] { 0, "Zero quantity" };
            yield return new object[] { 1, "Minimum quantity" };
            yield return new object[] { 999, "Maximum quantity" };
            yield return new object[] { 1000, "Boundary maximum" };
        }

        /// <summary>
        /// Get error scenario test data
        /// </summary>
        public static IEnumerable<object[]> GetErrorScenarioData()
        {
            yield return new object[] { "NetworkError", "Simulated network failure" };
            yield return new object[] { "TimeoutError", "Request timeout scenario" };
            yield return new object[] { "ServerError", "500 internal server error" };
            yield return new object[] { "NotFound", "404 page not found" };
        }
        #endregion
    }

    #region Builder Classes

    /// <summary>
    /// Product Builder implementing Builder pattern for flexible product creation
    /// </summary>
    public class ProductBuilder
    {
        private string _name = "Default Product";
        private string _price = "Rs. 0";
        private string _category = "Uncategorized";
        private string _brand = "Unknown";
        private string _availability = "In Stock";
        private string _condition = "New";

        public ProductBuilder WithName(string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            return this;
        }

        public ProductBuilder WithPrice(string price)
        {
            _price = price ?? throw new ArgumentNullException(nameof(price));
            return this;
        }

        public ProductBuilder WithCategory(string category)
        {
            _category = category ?? throw new ArgumentNullException(nameof(category));
            return this;
        }

        public ProductBuilder WithBrand(string brand)
        {
            _brand = brand ?? throw new ArgumentNullException(nameof(brand));
            return this;
        }

        public ProductBuilder WithAvailability(string availability)
        {
            _availability = availability ?? throw new ArgumentNullException(nameof(availability));
            return this;
        }

        public ProductBuilder WithDescription(string description)
        {
            _condition = description ?? throw new ArgumentNullException(nameof(description));
            return this;
        }

        public Product Build()
        {
            return new Product
            {
                Name = _name,
                Price = decimal.TryParse(_price.Replace("Rs. ", ""), out var price) ? price : 0,
                Category = _category,
                Brand = _brand,
                IsAvailable = _availability == "In Stock",
                Description = _condition
            };
        }

        /// <summary>
        /// Reset builder to default state for reuse
        /// </summary>
        public ProductBuilder Reset()
        {
            _name = "Default Product";
            _price = "Rs. 0";
            _category = "Uncategorized";
            _brand = "Unknown";
            _availability = "In Stock";
            _condition = "New";
            return this;
        }
    }

    /// <summary>
    /// Test Configuration Builder for creating test configurations
    /// </summary>
    public class TestConfigurationBuilder
    {
        private string _browser = "chromium";
        private bool _headless = false;
        private int _timeout = 30000;
        private int _slowMo = 0;
        private (int width, int height) _viewport = (1920, 1080);
        private Dictionary<string, object> _additionalOptions = new();

        public TestConfigurationBuilder WithBrowser(string browser)
        {
            _browser = browser ?? throw new ArgumentNullException(nameof(browser));
            return this;
        }

        public TestConfigurationBuilder WithHeadless(bool headless)
        {
            _headless = headless;
            return this;
        }

        public TestConfigurationBuilder WithTimeout(int timeout)
        {
            _timeout = timeout > 0 ? timeout : throw new ArgumentException("Timeout must be positive", nameof(timeout));
            return this;
        }

        public TestConfigurationBuilder WithSlowMotion(int slowMo)
        {
            _slowMo = slowMo >= 0 ? slowMo : throw new ArgumentException("SlowMo must be non-negative", nameof(slowMo));
            return this;
        }

        public TestConfigurationBuilder WithViewport(int width, int height)
        {
            _viewport = (width > 0 ? width : throw new ArgumentException("Width must be positive", nameof(width)),
                        height > 0 ? height : throw new ArgumentException("Height must be positive", nameof(height)));
            return this;
        }

        public TestConfigurationBuilder WithOption(string key, object value)
        {
            _additionalOptions[key] = value;
            return this;
        }

        public BrowserConfiguration Build()
        {
            return new BrowserConfiguration
            {
                Browser = _browser,
                Headless = _headless,
                Timeout = _timeout,
                SlowMotion = _slowMo,
                ViewportWidth = _viewport.width,
                ViewportHeight = _viewport.height,
                AdditionalOptions = new Dictionary<string, object>(_additionalOptions)
            };
        }
    }
    #endregion

    #region Configuration Model

    /// <summary>
    /// Browser Configuration model
    /// </summary>
    public class BrowserConfiguration
    {
        public string Browser { get; set; } = "chromium";
        public bool Headless { get; set; } = false;
        public int Timeout { get; set; } = 30000;
        public int SlowMotion { get; set; } = 0;
        public int ViewportWidth { get; set; } = 1920;
        public int ViewportHeight { get; set; } = 1080;
        public Dictionary<string, object> AdditionalOptions { get; set; } = new();
    }
    #endregion
} 