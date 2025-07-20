using Microsoft.Playwright;
using AutomationExercise.Tests.Helpers;

namespace AutomationExercise.Tests.Pages;

/// <summary>
/// Page Object for the Home page of automationexercise.com
/// </summary>
public class HomePage : BasePage
{
    // Page selectors from configuration
    private readonly HomePageSelectors _selectors;
    
    // Navigation
    private readonly string _productsLink;
    private readonly string _cartLink;
    private readonly string _homeLink;
    private readonly string _signupLoginLink;
    private readonly string _contactUsLink;
    private readonly string _testCasesLink;
    
    // Main sections
    private readonly string _featuresItems;
    private readonly string _categorySection;
    private readonly string _brandsSection;
    
    // Product elements  
    private readonly string _productItems;
    private readonly string _addToCartButtons;
    private readonly string _viewProductButtons;
    
    // Cart modal
    private readonly string _cartModal;
    private readonly string _viewCartButton;
    private readonly string _continueShoppingButton;
    private readonly string _cartModalCloseButton;

    public HomePage(IPage page) : base(page) 
    {
        _selectors = ConfigManager.Selectors.HomePage;
        
        // Initialize navigation selectors
        _productsLink = _selectors.Navigation.ProductsLink;
        _cartLink = _selectors.Navigation.CartLink;
        _homeLink = _selectors.Navigation.HomeLink;
        _signupLoginLink = _selectors.Navigation.SignupLoginLink;
        _contactUsLink = _selectors.Navigation.ContactUsLink;
        _testCasesLink = _selectors.Navigation.TestCasesLink;
        
        // Initialize section selectors
        _featuresItems = _selectors.Sections.FeaturesItems;
        _categorySection = _selectors.Sections.CategorySection;
        _brandsSection = _selectors.Sections.BrandsSection;
        
        // Initialize product selectors
        _productItems = _selectors.Products.ProductItems;
        _addToCartButtons = _selectors.Products.AddToCartButtons;
        _viewProductButtons = _selectors.Products.ViewProductButtons;
        
        // Initialize cart modal selectors
        _cartModal = _selectors.CartModal.Container;
        _viewCartButton = _selectors.CartModal.ViewCartButton;
        _continueShoppingButton = _selectors.CartModal.ContinueShoppingButton;
        _cartModalCloseButton = _selectors.CartModal.CloseButton;
    }

    /// <summary>
    /// Navigate to the home page
    /// </summary>
    public async Task NavigateAsync()
    {
        await NavigateToAsync(_baseUrl);
        await HandleCookieConsentAsync();
        await WaitForPageLoadAsync();
    }

    /// <summary>
    /// Navigate to Products page
    /// </summary>
    public async Task<ProductsPage> NavigateToProductsAsync()
    {
        await ClickAsync(_productsLink);
        await WaitForPageLoadAsync();
        return new ProductsPage(_page);
    }

    /// <summary>
    /// Navigate to Cart page
    /// </summary>
    public async Task<CartPage> NavigateToCartAsync()
    {
        await ClickAsync(_cartLink);
        await WaitForPageLoadAsync();
        return new CartPage(_page);
    }

    /// <summary>
    /// Add a product to cart by index (from featured items)
    /// </summary>
    public async Task AddProductToCartAsync(int productIndex = 0)
    {
        var addToCartButtons = await _page.QuerySelectorAllAsync(_addToCartButtons);
        if (addToCartButtons.Count > productIndex)
        {
            // Scroll the product into view first
            var productSelector = $"{_productItems}:nth-child({productIndex + 1})";
            await ScrollIntoViewAsync(productSelector);
            await Task.Delay(500); // Wait for any animations
            
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
    /// Handle cart modal after adding product
    /// </summary>
    public async Task<CartPage> ViewCartFromModalAsync()
    {
        await ClickAsync(_viewCartButton);
        await WaitForPageLoadAsync();
        return new CartPage(_page);
    }

    /// <summary>
    /// Continue shopping from cart modal
    /// </summary>
    public async Task ContinueShoppingAsync()
    {
        await ClickAsync(_continueShoppingButton);
        await WaitForElementAsync(_featuresItems);
    }

    /// <summary>
    /// Close cart modal
    /// </summary>
    public async Task CloseCartModalAsync()
    {
        if (await IsVisibleAsync(_cartModal))
        {
            await ClickAsync(_cartModalCloseButton);
        }
    }

    /// <summary>
    /// Get all featured product names
    /// </summary>
    public async Task<IReadOnlyList<string>> GetFeaturedProductNamesAsync()
    {
        return await GetAllTextsAsync($"{_productItems} p");
    }

    /// <summary>
    /// Get all featured product prices
    /// </summary>
    public async Task<IReadOnlyList<string>> GetFeaturedProductPricesAsync()
    {
        return await GetAllTextsAsync($"{_productItems} h2");
    }

    /// <summary>
    /// Check if cart modal is visible
    /// </summary>
    public async Task<bool> IsCartModalVisibleAsync()
    {
        return await IsVisibleAsync(_cartModal);
    }

    /// <summary>
    /// Verify home page is loaded
    /// </summary>
    public async Task<bool> IsPageLoadedAsync()
    {
        return await IsVisibleAsync(_featuresItems) && 
               await IsVisibleAsync(_categorySection);
    }

    /// <summary>
    /// Get the page title
    /// </summary>
    public async Task<string> GetPageTitleAsync()
    {
        return await _page.TitleAsync();
    }

    /// <summary>
    /// Verify navigation menu is visible
    /// </summary>
    public async Task<bool> IsNavigationVisibleAsync()
    {
        return await IsVisibleAsync(_productsLink) &&
               await IsVisibleAsync(_cartLink) &&
               await IsVisibleAsync(_homeLink);
    }
} 