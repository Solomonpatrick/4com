using Microsoft.Playwright;
using AutomationExercise.Tests.Models;

namespace AutomationExercise.Tests.Pages;

/// <summary>
/// Page Object for the Cart page of automationexercise.com
/// </summary>
public class CartPage : BasePage
{
    // Page selectors
    private readonly string _cartTable = "#cart_info_table";
    private readonly string _cartItems = ".cart_info tbody tr";
    private readonly string _emptyCartMessage = ".cart_info p";
    
    // Cart item elements
    private readonly string _productImage = ".cart_product img";
    private readonly string _productName = ".cart_description h4 a";
    private readonly string _productPrice = ".cart_price p";
    private readonly string _productQuantity = ".cart_quantity button";
    private readonly string _productTotal = ".cart_total_price";
    private readonly string _removeButton = ".cart_quantity_delete";
    
    // Cart summary
    private readonly string _cartTotalSection = ".cart_total_area";
    private readonly string _subTotal = ".cart_total_price";
    private readonly string _totalAmount = ".cart_total_price";
    
    // Action buttons
    private readonly string _proceedToCheckoutButton = "a[href='/checkout']";
    private readonly string _continueShoppingButton = ".close-modal";
    
    // Navigation
    private readonly string _homeLink = "a[href='/']";
    private readonly string _productsLink = "a[href='/products']";

    public CartPage(IPage page) : base(page) { }

    /// <summary>
    /// Navigate to cart page directly
    /// </summary>
    public async Task NavigateAsync()
    {
        await NavigateToAsync($"{_baseUrl}/view_cart");
        await HandleCookieConsentAsync();
        await WaitForPageLoadAsync();
    }

    /// <summary>
    /// Get all cart items
    /// </summary>
    public async Task<IReadOnlyList<CartItem>> GetCartItemsAsync()
    {
        var cartItemElements = await _page.QuerySelectorAllAsync(_cartItems);
        var cartItems = new List<CartItem>();

        for (int i = 0; i < cartItemElements.Count; i++)
        {
            var itemElement = cartItemElements[i];
            
            var nameElement = await itemElement.QuerySelectorAsync(_productName);
            var priceElement = await itemElement.QuerySelectorAsync(_productPrice);
            var quantityElement = await itemElement.QuerySelectorAsync(_productQuantity);
            var totalElement = await itemElement.QuerySelectorAsync(_productTotal);
            var imageElement = await itemElement.QuerySelectorAsync(_productImage);

            var name = await nameElement?.TextContentAsync() ?? "";
            var priceText = await priceElement?.TextContentAsync() ?? "";
            var quantityText = await quantityElement?.TextContentAsync() ?? "";
            var totalText = await totalElement?.TextContentAsync() ?? "";
            var imageUrl = await imageElement?.GetAttributeAsync("src") ?? "";

            cartItems.Add(new CartItem
            {
                Name = name.Trim(),
                Price = ExtractPriceFromText(priceText),
                Quantity = ExtractQuantityFromText(quantityText),
                Total = ExtractPriceFromText(totalText),
                ImageUrl = imageUrl,
                Index = i
            });
        }

        return cartItems;
    }

    /// <summary>
    /// Remove item from cart by index
    /// </summary>
    public async Task RemoveItemAsync(int itemIndex)
    {
        var removeButtons = await _page.QuerySelectorAllAsync(_removeButton);
        if (removeButtons.Count > itemIndex)
        {
            var initialCount = removeButtons.Count;
            await removeButtons[itemIndex].ClickAsync();
            
            // Wait for the cart to update (either item removed or page reloaded)
            await Task.Delay(2000);
            await WaitForPageLoadAsync();
        }
        else
        {
            throw new ArgumentException($"Item index {itemIndex} is out of range. Available items: {removeButtons.Count}");
        }
    }

    /// <summary>
    /// Remove item from cart by product name
    /// </summary>
    public async Task RemoveItemByNameAsync(string productName)
    {
        var cartItems = await GetCartItemsAsync();
        var item = cartItems.FirstOrDefault(i => i.Name.Contains(productName, StringComparison.OrdinalIgnoreCase));
        
        if (item != null)
        {
            await RemoveItemAsync(item.Index);
        }
        else
        {
            throw new ArgumentException($"Item with name '{productName}' not found in cart");
        }
    }

    /// <summary>
    /// Get cart total amount
    /// </summary>
    public async Task<decimal> GetCartTotalAsync()
    {
        try
        {
            var totalText = await GetTextAsync(_totalAmount);
            return ExtractPriceFromText(totalText);
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Get cart subtotal
    /// </summary>
    public async Task<decimal> GetCartSubtotalAsync()
    {
        try
        {
            var subtotalText = await GetTextAsync(_subTotal);
            return ExtractPriceFromText(subtotalText);
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Get the number of items in cart
    /// </summary>
    public async Task<int> GetCartItemCountAsync()
    {
        var cartItems = await _page.QuerySelectorAllAsync(_cartItems);
        return cartItems.Count;
    }

    /// <summary>
    /// Verify cart is empty
    /// </summary>
    public async Task<bool> IsCartEmptyAsync()
    {
        return await GetCartItemCountAsync() == 0 || 
               await IsVisibleAsync(_emptyCartMessage);
    }

    /// <summary>
    /// Verify specific product is in cart
    /// </summary>
    public async Task<bool> IsProductInCartAsync(string productName)
    {
        var cartItems = await GetCartItemsAsync();
        return cartItems.Any(item => item.Name.Contains(productName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verify cart calculations are correct
    /// </summary>
    public async Task<bool> AreCartCalculationsCorrectAsync()
    {
        var cartItems = await GetCartItemsAsync();
        var expectedTotal = cartItems.Sum(item => item.Price * item.Quantity);
        var actualTotal = await GetCartTotalAsync();
        
        // Allow for small decimal differences due to rounding
        return Math.Abs(expectedTotal - actualTotal) < 0.01m;
    }

    /// <summary>
    /// Proceed to checkout
    /// </summary>
    public async Task ProceedToCheckoutAsync()
    {
        await ClickAsync(_proceedToCheckoutButton);
        await WaitForPageLoadAsync();
    }

    /// <summary>
    /// Continue shopping
    /// </summary>
    public async Task<ProductsPage> ContinueShoppingAsync()
    {
        await ClickAsync(_continueShoppingButton);
        await WaitForPageLoadAsync();
        return new ProductsPage(_page);
    }

    /// <summary>
    /// Navigate back to home page
    /// </summary>
    public async Task<HomePage> NavigateToHomeAsync()
    {
        await ClickAsync(_homeLink);
        await WaitForPageLoadAsync();
        return new HomePage(_page);
    }

    /// <summary>
    /// Navigate to products page
    /// </summary>
    public async Task<ProductsPage> NavigateToProductsAsync()
    {
        await ClickAsync(_productsLink);
        await WaitForPageLoadAsync();
        return new ProductsPage(_page);
    }

    /// <summary>
    /// Verify cart page is loaded
    /// </summary>
    public async Task<bool> IsPageLoadedAsync()
    {
        return await IsVisibleAsync(_cartTable) || await IsVisibleAsync(_emptyCartMessage);
    }

    /// <summary>
    /// Get cart summary information
    /// </summary>
    public async Task<CartSummary> GetCartSummaryAsync()
    {
        var cartItems = await GetCartItemsAsync();
        var total = await GetCartTotalAsync();
        var itemCount = cartItems.Count;
        var totalQuantity = cartItems.Sum(item => item.Quantity);

        // For this website, subtotal and total are typically the same
        var subtotal = total > 0 ? total : cartItems.Sum(item => item.Total);

        return new CartSummary
        {
            ItemCount = itemCount,
            TotalQuantity = totalQuantity,
            Subtotal = subtotal,
            Total = total > 0 ? total : subtotal,
            Items = cartItems
        };
    }

    /// <summary>
    /// Validate that each item's total is correct (price * quantity)
    /// </summary>
    public async Task<bool> ValidateItemTotalsAsync()
    {
        var cartItems = await GetCartItemsAsync();
        
        foreach (var item in cartItems)
        {
            var expectedTotal = item.Price * item.Quantity;
            if (Math.Abs(expectedTotal - item.Total) >= 0.01m)
            {
                return false;
            }
        }
        
        return true;
    }

    /// <summary>
    /// Clear entire cart
    /// </summary>
    public async Task ClearCartAsync()
    {
        var cartItems = await GetCartItemsAsync();
        
        // Remove items one by one (starting from the end to avoid index issues)
        for (int i = cartItems.Count - 1; i >= 0; i--)
        {
            await RemoveItemAsync(i);
        }
    }

    /// <summary>
    /// Get the page title
    /// </summary>
    public async Task<string> GetPageTitleAsync()
    {
        return await _page.TitleAsync();
    }

    /// <summary>
    /// Extract numeric price from price text
    /// </summary>
    private static decimal ExtractPriceFromText(string priceText)
    {
        var numbers = System.Text.RegularExpressions.Regex.Match(priceText, @"\d+");
        return numbers.Success ? decimal.Parse(numbers.Value) : 0;
    }

    /// <summary>
    /// Extract quantity from quantity text
    /// </summary>
    private static int ExtractQuantityFromText(string quantityText)
    {
        var numbers = System.Text.RegularExpressions.Regex.Match(quantityText, @"\d+");
        return numbers.Success ? int.Parse(numbers.Value) : 0;
    }
} 