using AutomationExercise.Tests.Helpers;

namespace AutomationExercise.Tests.Tests;

/// <summary>
/// Test suite for shopping cart validation functionality
/// </summary>
[TestFixture]
[Category("Cart")]
public class CartValidationTests : BaseTest
{
    [Test]
    [Description("Verify empty cart state and navigation")]
    public async Task NavigateToCart_WhenEmpty_ShouldShowEmptyCartState()
    {
        // Arrange
        LogTestInfo("Starting empty cart navigation test");
        
        // Act
        await NavigateToCartPageAsync();

        // Assert
        var isCartEmpty = await _cartPage!.IsCartEmptyAsync();
        isCartEmpty.Should().BeTrue("Cart should be empty initially");
        
        var cartItemCount = await _cartPage.GetCartItemCountAsync();
        cartItemCount.Should().Be(0, "Empty cart should have 0 items");

        LogTestInfo("Empty cart navigation test completed");
    }

    [Test]
    [Description("Verify adding single product to cart")]
    public async Task AddSingleProductToCart_ShouldAddSuccessfully()
    {
        // Arrange
        LogTestInfo("Starting single product add to cart test");
        await NavigateToHomePageAsync();

        // Act - Add first featured product
        await _homePage!.AddProductToCartAsync(0);
        var cartPage = await _homePage.ViewCartFromModalAsync();

        // Assert
        var cartItems = await cartPage.GetCartItemsAsync();
        cartItems.Should().HaveCount(1, "Cart should contain exactly one item");

        var cartItemCount = await cartPage.GetCartItemCountAsync();
        cartItemCount.Should().Be(1, "Cart count should be 1");

        var firstItem = cartItems.First();
        firstItem.Name.Should().NotBeNullOrEmpty("Product name should be present");
        firstItem.Price.Should().BeGreaterThan(0, "Product price should be greater than 0");
        firstItem.Quantity.Should().Be(1, "Default quantity should be 1");
        firstItem.Total.Should().Be(firstItem.Price * firstItem.Quantity, "Total should equal price * quantity");

        LogTestInfo($"Single product add test completed. Added: {firstItem.Name}");
    }

    [Test]
    [Description("Verify adding multiple different products to cart")]
    public async Task AddMultipleProductsToCart_ShouldAddAllSuccessfully()
    {
        // Arrange
        LogTestInfo("Starting multiple products add to cart test");
        await NavigateToHomePageAsync();
        var productsToAdd = 3;

        // Act - Add multiple products
        for (int i = 0; i < productsToAdd; i++)
        {
            await _homePage!.AddProductToCartAsync(i);
            await _homePage.ContinueShoppingAsync();
        }

        var cartPage = await _homePage.NavigateToCartAsync();

        // Assert
        var cartItems = await cartPage.GetCartItemsAsync();
        cartItems.Should().NotBeEmpty("Cart should contain items");

        var cartSummary = await cartPage.GetCartSummaryAsync();
        cartSummary.TotalQuantity.Should().Be(productsToAdd, "Total quantity should equal number of products added");
        cartSummary.ItemCount.Should().BeGreaterThan(0, "Cart should have at least one item type");

        // Verify each item has valid data
        foreach (var item in cartItems)
        {
            item.Name.Should().NotBeNullOrEmpty("Each product should have a name");
            item.Price.Should().BeGreaterThan(0, "Each product should have a price > 0");
            item.Quantity.Should().BeGreaterThan(0, "Each product should have quantity > 0");
            item.IsCalculationCorrect().Should().BeTrue("Each item's total should be calculated correctly");
        }

        LogTestInfo($"Multiple products add test completed. Added {cartItems.Count} items");
    }

    [Test]
    [Description("Verify cart calculations are accurate")]
    public async Task CartCalculations_ShouldBeAccurate()
    {
        // Arrange
        LogTestInfo("Starting cart calculations validation test");
        await NavigateToHomePageAsync();

        // Act - Add products to cart
        await _homePage!.AddProductToCartAsync(0);
        await _homePage.ContinueShoppingAsync();
        await _homePage.AddProductToCartAsync(1);
        
        var cartPage = await _homePage.ViewCartFromModalAsync();

        // Assert
        var cartItems = await cartPage.GetCartItemsAsync();
        cartItems.Should().NotBeEmpty("Cart should have items for calculation test");

        // Verify individual item calculations
        var itemCalculationsCorrect = await cartPage.ValidateItemTotalsAsync();
        itemCalculationsCorrect.Should().BeTrue("All individual item calculations should be correct");

        // Verify overall cart calculations
        var cartCalculationsCorrect = await cartPage.AreCartCalculationsCorrectAsync();
        cartCalculationsCorrect.Should().BeTrue("Overall cart calculations should be correct");

        var cartSummary = await cartPage.GetCartSummaryAsync();
        cartSummary.AreCalculationsCorrect().Should().BeTrue("Cart summary calculations should be accurate");

        // Manual verification
        var expectedSubtotal = cartItems.Sum(item => item.Total);
        var actualSubtotal = await cartPage.GetCartSubtotalAsync();
        actualSubtotal.Should().Be(expectedSubtotal, "Subtotal should match sum of item totals");

        LogTestInfo($"Cart calculations test completed. Subtotal: {actualSubtotal}");
    }

    [Test]
    [Description("Verify removing items from cart")]
    public async Task RemoveItemFromCart_ShouldRemoveSuccessfully()
    {
        // Arrange
        LogTestInfo("Starting remove item from cart test");
        await NavigateToHomePageAsync();

        // Add two products first
        await _homePage!.AddProductToCartAsync(0);
        await _homePage.ContinueShoppingAsync();
        await _homePage.AddProductToCartAsync(1);
        
        var cartPage = await _homePage.ViewCartFromModalAsync();
        var initialItems = await cartPage.GetCartItemsAsync();
        var initialCount = initialItems.Count;

        // Act - Remove first item
        await cartPage.RemoveItemAsync(0);

        // Assert
        var remainingItems = await cartPage.GetCartItemsAsync();
        remainingItems.Should().HaveCount(initialCount - 1, "Cart should have one less item after removal");

        // Verify calculations are still correct after removal
        var calculationsCorrect = await cartPage.AreCartCalculationsCorrectAsync();
        calculationsCorrect.Should().BeTrue("Cart calculations should remain correct after item removal");

        LogTestInfo($"Remove item test completed. Items before: {initialCount}, after: {remainingItems.Count}");
    }

    [Test]
    [Description("Verify clearing entire cart")]
    public async Task ClearCart_ShouldRemoveAllItems()
    {
        // Arrange
        LogTestInfo("Starting clear cart test");
        await NavigateToHomePageAsync();

        // Add multiple products
        await _homePage!.AddProductToCartAsync(0);
        await _homePage.ContinueShoppingAsync();
        await _homePage.AddProductToCartAsync(1);
        await _homePage.ContinueShoppingAsync();
        await _homePage.AddProductToCartAsync(2);
        
        var cartPage = await _homePage.ViewCartFromModalAsync();

        // Act - Clear cart
        await cartPage.ClearCartAsync();

        // Assert
        var isCartEmpty = await cartPage.IsCartEmptyAsync();
        isCartEmpty.Should().BeTrue("Cart should be empty after clearing");

        var cartItemCount = await cartPage.GetCartItemCountAsync();
        cartItemCount.Should().Be(0, "Cart should have 0 items after clearing");

        LogTestInfo("Clear cart test completed");
    }

    [Test]
    [Description("Verify cart persistence across navigation")]
    public async Task CartPersistence_AcrossNavigation_ShouldMaintainItems()
    {
        // Arrange
        LogTestInfo("Starting cart persistence test");
        await NavigateToHomePageAsync();

        // Act - Add product and navigate away then back
        await _homePage!.AddProductToCartAsync(0);
        var cartPage = await _homePage.ViewCartFromModalAsync();
        var initialItems = await cartPage.GetCartItemsAsync();

        // Navigate to products page and back to cart
        var productsPage = await cartPage.NavigateToProductsAsync();
        await _cartPage!.NavigateAsync();

        // Assert
        var persistedItems = await _cartPage!.GetCartItemsAsync();
        persistedItems.Should().HaveCount(initialItems.Count, "Cart items should persist across navigation");

        if (persistedItems.Any() && initialItems.Any())
        {
            persistedItems.First().Name.Should().Be(initialItems.First().Name, 
                "First item should be the same after navigation");
        }

        LogTestInfo("Cart persistence test completed");
    }

    [Test]
    [Description("Verify search, add to cart, and cart validation workflow")]
    public async Task SearchAddToCartValidation_EndToEndWorkflow_ShouldWorkCorrectly()
    {
        // Arrange
        LogTestInfo("Starting end-to-end search and cart validation workflow");
        await NavigateToProductsPageAsync();
        var searchTerm = TestDataGenerator.GetRandomSearchTerm();

        // Act - Complete workflow
        await _productsPage!.SearchProductsAsync(searchTerm);
        var searchResults = await _productsPage.GetSearchResultsAsync();
        
        if (searchResults.Any())
        {
            var selectedProduct = searchResults.First();
            await _productsPage.AddProductToCartByNameAsync(selectedProduct.Name);
            var cartPage = await _productsPage.ViewCartFromModalAsync();

            // Assert
            var cartItems = await cartPage.GetCartItemsAsync();
            cartItems.Should().NotBeEmpty("Cart should contain the searched and added product");

            var addedProduct = cartItems.FirstOrDefault(item => 
                item.Name.Contains(selectedProduct.Name, StringComparison.OrdinalIgnoreCase));
            addedProduct.Should().NotBeNull("The specific searched product should be in the cart");

            var cartSummary = await cartPage.GetCartSummaryAsync();
            cartSummary.AreCalculationsCorrect().Should().BeTrue("Cart calculations should be correct");

            LogTestInfo($"End-to-end workflow completed successfully for product: {selectedProduct.Name}");
        }
        else
        {
            Assert.Inconclusive($"No products found for search term '{searchTerm}' to complete workflow test");
        }
    }

    [Test]
    [Description("Verify cart validation with edge cases")]
    public async Task CartValidation_EdgeCases_ShouldHandleGracefully()
    {
        // Arrange
        LogTestInfo("Starting cart edge cases validation test");
        await NavigateToCartPageAsync();

        // Act & Assert - Test empty cart operations
        var isInitiallyEmpty = await _cartPage!.IsCartEmptyAsync();
        isInitiallyEmpty.Should().BeTrue("Cart should be empty initially");

        // Try to get cart total when empty
        var emptyCartTotal = await _cartPage.GetCartTotalAsync();
        emptyCartTotal.Should().Be(0, "Empty cart total should be 0");

        // Try to get cart summary when empty
        var emptyCartSummary = await _cartPage.GetCartSummaryAsync();
        emptyCartSummary.IsEmpty.Should().BeTrue("Empty cart summary should indicate empty state");

        LogTestInfo("Cart edge cases validation test completed");
    }

    [Test]
    [Description("Verify cart UI elements and interactions")]
    public async Task CartUIElements_ShouldBeVisibleAndInteractive()
    {
        // Arrange
        LogTestInfo("Starting cart UI elements test");
        await NavigateToHomePageAsync();

        // Add a product to test UI elements
        await _homePage!.AddProductToCartAsync(0);
        var cartPage = await _homePage.ViewCartFromModalAsync();

        // Assert - Verify cart page is properly loaded
        var isPageLoaded = await cartPage.IsPageLoadedAsync();
        isPageLoaded.Should().BeTrue("Cart page should be loaded properly");

        var pageTitle = await cartPage.GetPageTitleAsync();
        pageTitle.Should().NotBeNullOrEmpty("Cart page should have a title");

        // Test navigation elements
        var homePage = await cartPage.NavigateToHomeAsync();
        var homePageLoaded = await homePage.IsPageLoadedAsync();
        homePageLoaded.Should().BeTrue("Should be able to navigate back to home page");

        LogTestInfo("Cart UI elements test completed");
    }

    [Test]
    [Description("Verify cart performance with multiple operations")]
    public async Task CartPerformance_MultipleOperations_ShouldCompleteWithinTimeLimit()
    {
        // Arrange
        LogTestInfo("Starting cart performance test");
        await NavigateToHomePageAsync();
        var maxTimePerOperation = TimeSpan.FromSeconds(3);
        var operationsCount = 5;

        // Act & Assert - Add multiple items and measure time
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        for (int i = 0; i < operationsCount; i++)
        {
            var operationStart = System.Diagnostics.Stopwatch.StartNew();
            await _homePage!.AddProductToCartAsync(i);
            await _homePage.ContinueShoppingAsync();
            operationStart.Stop();

            operationStart.Elapsed.Should().BeLessThan(maxTimePerOperation, 
                $"Add to cart operation {i + 1} should complete within time limit");
        }

        stopwatch.Stop();
        
        var cartPage = await _homePage.NavigateToCartAsync();
        var finalItems = await cartPage.GetCartItemsAsync();
        
        finalItems.Should().NotBeEmpty("Cart should contain added products");
        var totalQuantity = finalItems.Sum(item => item.Quantity);
        totalQuantity.Should().Be(operationsCount, "Total quantity should equal number of operations");
        
        LogTestInfo($"Cart performance test completed in {stopwatch.Elapsed.TotalMilliseconds}ms for {operationsCount} operations");
    }
} 