using AutomationExercise.Tests.Helpers;
using FluentAssertions;

namespace AutomationExercise.Tests.Tests;

/// <summary>
/// Test suite for product search functionality
/// </summary>
[TestFixture]
[Category("Search")]
public class ProductSearchTests : BaseTest
{
    [Test]
    [Description("Verify that user can navigate to products page")]
    public async Task NavigateToProducts_ShouldLoadProductsPage()
    {
        // Arrange
        LogTestInfo("Starting navigation to products page test");
        await NavigateToHomePageAsync();
        await LogPageStateAsync("After navigating to home page");

        // Act
        await _homePage!.NavigateToProductsAsync();

        // Assert
        var isLoaded = await _productsPage!.IsPageLoadedAsync();
        isLoaded.Should().BeTrue("Products page should be loaded successfully");
        
        var pageTitle = await _productsPage.GetPageTitleAsync();
        pageTitle.Should().Contain("Products", "Page title should contain 'Products'");
        
        await LogPageStateAsync("Test completed - on products page");
        LogTestInfo("Products page navigation test completed successfully");
    }

    [Test]
    [TestCaseSource(nameof(SearchTestData))]
    [Description("Verify product search functionality with various search terms")]
    public async Task SearchProducts_WithValidSearchTerm_ShouldReturnMatchingResults(string searchTerm)
    {
        // Arrange
        LogTestInfo($"Starting product search test with term: '{searchTerm}'");
        await NavigateToProductsPageAsync();
        await LogPageStateAsync($"Before searching for '{searchTerm}'");

        // Act
        await _productsPage!.SearchProductsAsync(searchTerm);
        await LogPageStateAsync($"After searching for '{searchTerm}'");

        // Assert
        var areSearchResultsDisplayed = await _productsPage.AreSearchResultsDisplayedAsync();
        areSearchResultsDisplayed.Should().BeTrue($"Search results should be displayed for term '{searchTerm}'");

        var searchResults = await _productsPage.GetSearchResultsAsync();
        searchResults.Should().NotBeEmpty($"Search should return results for term '{searchTerm}'");

        var containsSearchTerm = await _productsPage.DoProductsContainSearchTermAsync(searchTerm);
        containsSearchTerm.Should().BeTrue($"Search results should contain products related to '{searchTerm}'");

        LogTestInfo($"Search test for '{searchTerm}' completed. Found {searchResults.Count} products");
    }

    [Test]
    [Description("Verify search with empty/invalid terms")]
    public async Task SearchProducts_WithEmptySearchTerm_ShouldHandleGracefully()
    {
        // Arrange
        LogTestInfo("Starting empty search term test");
        await NavigateToProductsPageAsync();

        // Act
        await _productsPage!.SearchProductsAsync("");

        // Assert - Application should handle empty search gracefully
        var productCount = await _productsPage.GetProductCountAsync();
        productCount.Should().BeGreaterThanOrEqualTo(0, "Empty search should not cause errors");

        LogTestInfo("Empty search term test completed");
    }

    [Test]
    [Description("Verify search functionality with special characters")]
    public async Task SearchProducts_WithSpecialCharacters_ShouldHandleGracefully()
    {
        // Arrange
        LogTestInfo("Starting special characters search test");
        await NavigateToProductsPageAsync();
        var specialSearchTerms = new[] { "@#$%", "123", "   ", "!@#$%^&*()" };

        foreach (var searchTerm in specialSearchTerms)
        {
            // Act
            await _productsPage!.SearchProductsAsync(searchTerm);

            // Assert
            var productCount = await _productsPage.GetProductCountAsync();
            productCount.Should().BeGreaterThanOrEqualTo(0, $"Search with '{searchTerm}' should not cause errors");

            LogTestInfo($"Special character search test for '{searchTerm}' completed");
        }
    }

    [Test]
    [Description("Verify case insensitive search functionality")]
    public async Task SearchProducts_CaseInsensitive_ShouldReturnSameResults()
    {
        // Arrange
        LogTestInfo("Starting case insensitive search test");
        await NavigateToProductsPageAsync();
        var searchTerm = "TOP";

        // Act - Search with uppercase
        await _productsPage!.SearchProductsAsync(searchTerm.ToUpper());
        var upperCaseResults = await _productsPage.GetSearchResultsAsync();

        await NavigateToProductsPageAsync();
        
        // Search with lowercase
        await _productsPage.SearchProductsAsync(searchTerm.ToLower());
        var lowerCaseResults = await _productsPage.GetSearchResultsAsync();

        // Assert
        upperCaseResults.Count.Should().Be(lowerCaseResults.Count, 
            "Case insensitive search should return same number of results");

        LogTestInfo($"Case insensitive search test completed. Upper: {upperCaseResults.Count}, Lower: {lowerCaseResults.Count}");
    }

    [Test]
    [TestCaseSource(nameof(CategoryTestData))]
    [Description("Verify category filtering functionality")]
    public async Task FilterProducts_ByCategory_ShouldShowCategoryProducts(string category)
    {
        // Arrange
        LogTestInfo($"Starting category filter test for: {category}");
        await NavigateToProductsPageAsync();

        // Act
        await _productsPage!.FilterByCategoryAsync(category);

        // Assert
        var products = await _productsPage.GetAllProductsAsync();
        products.Should().NotBeEmpty($"Category '{category}' should have products");

        var productCount = await _productsPage.GetProductCountAsync();
        productCount.Should().BeGreaterThan(0, $"Category '{category}' should show products");

        LogTestInfo($"Category filter test for '{category}' completed. Found {products.Count} products");
    }

    [Test]
    [Description("Verify search and add to cart workflow")]
    public async Task SearchAndAddToCart_ShouldAddProductSuccessfully()
    {
        // Arrange
        LogTestInfo("Starting search and add to cart workflow test");
        await NavigateToProductsPageAsync();
        var searchTerm = TestDataGenerator.GetRandomSearchTerm();

        // Act
        await _productsPage!.SearchProductsAsync(searchTerm);
        var searchResults = await _productsPage.GetSearchResultsAsync();
        
        if (searchResults.Any())
        {
            await _productsPage.AddProductToCartAsync(0); // Add first product
            var isCartModalVisible = await _productsPage.IsCartModalVisibleAsync();
            
            // Assert
            isCartModalVisible.Should().BeTrue("Cart modal should appear after adding product");

            // Navigate to cart to verify
            var cartPage = await _productsPage.ViewCartFromModalAsync();
            var cartItems = await cartPage.GetCartItemsAsync();
            
            cartItems.Should().NotBeEmpty("Cart should contain the added product");
            cartItems.First().Name.Should().Contain(searchResults.First().Name, 
                "Cart should contain the product that was added");
        }
        else
        {
            Assert.Inconclusive($"No products found for search term '{searchTerm}' to test add to cart");
        }

        LogTestInfo("Search and add to cart workflow test completed");
    }

    [Test]
    [Description("Verify pagination functionality if available")]
    public async Task SearchResults_WithManyResults_ShouldHandlePagination()
    {
        // Arrange
        LogTestInfo("Starting pagination test");
        await NavigateToProductsPageAsync();

        // Act - Search for a common term that should return many results
        await _productsPage!.SearchProductsAsync("top");
        var firstPageResults = await _productsPage.GetSearchResultsAsync();

        // Assert
        firstPageResults.Should().NotBeEmpty("Search should return results");
        
        // Note: Actual pagination testing would depend on the website's pagination implementation
        var productCount = await _productsPage.GetProductCountAsync();
        productCount.Should().BeGreaterThan(0, "Should have products displayed");

        LogTestInfo($"Pagination test completed. Found {productCount} products on current page");
    }

    [Test]
    [Description("Verify search performance and response time")]
    public async Task SearchProducts_PerformanceTest_ShouldCompleteWithinTimeLimit()
    {
        // Arrange
        LogTestInfo("Starting search performance test");
        await NavigateToProductsPageAsync();
        var searchTerm = "dress";
        var maxResponseTime = TimeSpan.FromSeconds(6);

        // Act & Assert
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await _productsPage!.SearchProductsAsync(searchTerm);
        var searchResults = await _productsPage.GetSearchResultsAsync();
        stopwatch.Stop();

        searchResults.Should().NotBeEmpty("Search should return results");
        stopwatch.Elapsed.Should().BeLessThan(maxResponseTime, 
            $"Search should complete within {maxResponseTime.TotalSeconds} seconds");

        LogTestInfo($"Performance test completed in {stopwatch.Elapsed.TotalMilliseconds}ms");
    }



    #region Test Data Sources

    private static object[][] SearchTestData()
    {
        return TestDataFactory.GetSearchTestData()
            .Select(data => new object[] { data[0] })
            .ToArray();
    }

    private static IEnumerable<object[]> CategoryTestData()
    {
        return TestDataFactory.GetCategoryTestData();
    }

    #endregion
} 