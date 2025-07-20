using Microsoft.Playwright;
using AutomationExercise.Tests.Models;
using AutomationExercise.Tests.Helpers;

namespace AutomationExercise.Tests.Pages;

/// <summary>
/// Page Object for the Products page of automationexercise.com
/// </summary>
public class ProductsPage : BasePage
{
    // Page selectors
    private readonly string _searchInput = "#search_product";
    private readonly string _searchButton = "#submit_search";
    private readonly string _allProductsTitle = ".title";
    
    // Product grid
    private readonly string _productItems = ".product-image-wrapper";
    private readonly string _productNames = ".productinfo p";
    private readonly string _productPrices = ".productinfo h2";
    private readonly string _addToCartButtons = "a[data-product-id].add-to-cart";
    private readonly string _viewProductButtons = "a[href*='product_details']";
    
    // Categories and filters
    private readonly string _categorySection = ".category-products";
    private readonly string _womenCategory = "a[href='#Women']";
    private readonly string _menCategory = "a[href='#Men']";
    private readonly string _kidsCategory = "a[href='#Kids']";
    private readonly string _categoryItems = ".panel-body a";
    
    // Brands
    private readonly string _brandsSection = ".brands_products";
    private readonly string _brandItems = ".brands_products a";
    
    // Cart modal
    private readonly string _cartModal = "#cartModal";
    private readonly string _viewCartButton = "#cartModal a[href='/view_cart']";
    private readonly string _continueShoppingButton = "#cartModal .close-modal";
    
    // Search results
    private readonly string _searchedProductsTitle = ".title";

    public ProductsPage(IPage page) : base(page) { }

    /// <summary>
    /// Navigate to the products page directly
    /// </summary>
    public async Task NavigateAsync()
    {
        await NavigateToAsync($"{_baseUrl}/products");
        await HandleCookieConsentAsync();
        await WaitForPageLoadAsync();
    }

    /// <summary>
    /// Search for products using the search functionality
    /// </summary>
    public async Task SearchProductsAsync(string searchTerm)
    {
        await FillAsync(_searchInput, searchTerm);
        await ClickAsync(_searchButton);
        
        // Wait for search results to load using centralized wait strategy
        await WaitStrategies.WaitForSearchResultsAsync(_page, _productItems);
    }

    /// <summary>
    /// Get all product information from the current page
    /// </summary>
    public async Task<IReadOnlyList<Product>> GetAllProductsAsync()
    {
        var productElements = await _page.QuerySelectorAllAsync(_productItems);
        var products = new List<Product>();

        for (int i = 0; i < productElements.Count; i++)
        {
            var nameElement = await productElements[i].QuerySelectorAsync(".productinfo p");
            var priceElement = await productElements[i].QuerySelectorAsync(".productinfo h2");
            var imageElement = await productElements[i].QuerySelectorAsync(".productinfo img");

            var name = await nameElement?.TextContentAsync() ?? "";
            var priceText = await priceElement?.TextContentAsync() ?? "";
            var imageUrl = await imageElement?.GetAttributeAsync("src") ?? "";

            // Extract price from text (e.g., "Rs. 500" -> "500")
            var price = ExtractPriceFromText(priceText);

            products.Add(new Product
            {
                Name = name.Trim(),
                Price = price,
                ImageUrl = imageUrl,
                Index = i
            });
        }

        return products;
    }

    /// <summary>
    /// Get products that match a search term
    /// </summary>
    public async Task<IReadOnlyList<Product>> GetSearchResultsAsync()
    {
        // Wait for search results to load
        await WaitForElementAsync(_searchedProductsTitle);
        return await GetAllProductsAsync();
    }

    /// <summary>
    /// Add a specific product to cart by index
    /// </summary>
    public async Task AddProductToCartAsync(int productIndex)
    {
        var addToCartButtons = await _page.QuerySelectorAllAsync(_addToCartButtons);
        if (addToCartButtons.Count > productIndex)
        {
            await ScrollIntoViewAsync($"{_productItems}:nth-child({productIndex + 1})");
            await Task.Delay(500); // Wait for animations
            
            // Click the button using JavaScript to bypass any overlay issues
            await addToCartButtons[productIndex].EvaluateAsync("element => element.click()");
            
            // Wait for the cart modal to become visible
            await _page.WaitForSelectorAsync(_cartModal + ".show", new PageWaitForSelectorOptions { Timeout = 10000 });
        }
        else
        {
            throw new ArgumentException($"Product index {productIndex} is out of range. Available products: {addToCartButtons.Count}");
        }
    }

    /// <summary>
    /// Add product to cart by name
    /// </summary>
    public async Task AddProductToCartByNameAsync(string productName)
    {
        var products = await GetAllProductsAsync();
        var product = products.FirstOrDefault(p => p.Name.Contains(productName, StringComparison.OrdinalIgnoreCase));
        
        if (product != null)
        {
            await AddProductToCartAsync(product.Index);
        }
        else
        {
            throw new ArgumentException($"Product with name '{productName}' not found");
        }
    }

    /// <summary>
    /// Continue shopping from cart modal
    /// </summary>
    public async Task ContinueShoppingAsync()
    {
        await ClickAsync(_continueShoppingButton);
        await WaitForPageLoadAsync();
    }

    /// <summary>
    /// View cart from modal
    /// </summary>
    public async Task<CartPage> ViewCartFromModalAsync()
    {
        await ClickAsync(_viewCartButton);
        await WaitForPageLoadAsync();
        return new CartPage(_page);
    }

    /// <summary>
    /// Filter products by category
    /// </summary>
    public async Task FilterByCategoryAsync(string categoryName)
    {
        var categorySelector = categoryName.ToLower() switch
        {
            "women" => _womenCategory,
            "men" => _menCategory,
            "kids" => _kidsCategory,
            _ => throw new ArgumentException($"Unknown category: {categoryName}")
        };

        await ClickAsync(categorySelector);
        await WaitForPageLoadAsync();
    }

    /// <summary>
    /// Filter products by brand
    /// </summary>
    public async Task FilterByBrandAsync(string brandName)
    {
        // Use Playwright's built-in text locator instead of CSS selector
        await _page.Locator(".brands_products").Locator("text=" + brandName).ClickAsync();
        await WaitForPageLoadAsync();
    }

    /// <summary>
    /// Get all available categories
    /// </summary>
    public async Task<IReadOnlyList<string>> GetAvailableCategoriesAsync()
    {
        return await GetAllTextsAsync(_categoryItems);
    }

    /// <summary>
    /// Get all available brands
    /// </summary>
    public async Task<IReadOnlyList<string>> GetAvailableBrandsAsync()
    {
        return await GetAllTextsAsync(_brandItems);
    }

    /// <summary>
    /// Verify products page is loaded
    /// </summary>
    public async Task<bool> IsPageLoadedAsync()
    {
        return await IsVisibleAsync(_allProductsTitle) && 
               await IsVisibleAsync(_searchInput);
    }

    /// <summary>
    /// Verify search results are displayed
    /// </summary>
    public async Task<bool> AreSearchResultsDisplayedAsync()
    {
        return await IsVisibleAsync(_searchedProductsTitle);
    }

    /// <summary>
    /// Get the count of products displayed
    /// </summary>
    public async Task<int> GetProductCountAsync()
    {
        var products = await _page.QuerySelectorAllAsync(_productItems);
        return products.Count;
    }

    /// <summary>
    /// Verify that products contain search term
    /// </summary>
    public async Task<bool> DoProductsContainSearchTermAsync(string searchTerm)
    {
        var products = await GetAllProductsAsync();
        return products.Any(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get the page title
    /// </summary>
    public async Task<string> GetPageTitleAsync()
    {
        return await _page.TitleAsync();
    }

    /// <summary>
    /// Check if cart modal is visible
    /// </summary>
    public async Task<bool> IsCartModalVisibleAsync()
    {
        return await IsVisibleAsync(_cartModal);
    }

    /// <summary>
    /// Extract numeric price from price text
    /// </summary>
    private static decimal ExtractPriceFromText(string priceText)
    {
        // Remove currency symbols and extract numbers
        var numbers = System.Text.RegularExpressions.Regex.Match(priceText, @"\d+");
        return numbers.Success ? decimal.Parse(numbers.Value) : 0;
    }
} 