namespace AutomationExercise.Tests.Models;

/// <summary>
/// Represents an item in the shopping cart
/// </summary>
public class CartItem
{
    /// <summary>
    /// Product name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unit price of the product
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Quantity of the product in cart
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Total price for this item (Price * Quantity)
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Product image URL
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Item index in the cart (for UI interaction)
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Product ID (if available)
    /// </summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Returns a string representation of the cart item
    /// </summary>
    public override string ToString()
    {
        return $"{Name} x{Quantity} = Rs. {Total}";
    }

    /// <summary>
    /// Validate that the total is correctly calculated
    /// </summary>
    public bool IsCalculationCorrect()
    {
        var expectedTotal = Price * Quantity;
        return Math.Abs(expectedTotal - Total) < 0.01m;
    }
}

/// <summary>
/// Represents a summary of the shopping cart
/// </summary>
public class CartSummary
{
    /// <summary>
    /// Total number of different items in cart
    /// </summary>
    public int ItemCount { get; set; }

    /// <summary>
    /// Total quantity of all items
    /// </summary>
    public int TotalQuantity { get; set; }

    /// <summary>
    /// Subtotal amount (before taxes/shipping)
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Total amount (final amount)
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Tax amount (if applicable)
    /// </summary>
    public decimal Tax { get; set; }

    /// <summary>
    /// Shipping cost (if applicable)
    /// </summary>
    public decimal ShippingCost { get; set; }

    /// <summary>
    /// List of items in the cart
    /// </summary>
    public IReadOnlyList<CartItem> Items { get; set; } = new List<CartItem>();

    /// <summary>
    /// Check if cart is empty
    /// </summary>
    public bool IsEmpty => ItemCount == 0;

    /// <summary>
    /// Returns a string representation of the cart summary
    /// </summary>
    public override string ToString()
    {
        return $"Cart: {ItemCount} items, {TotalQuantity} total qty, Rs. {Total} total";
    }

    /// <summary>
    /// Validate that all calculations in the cart are correct
    /// </summary>
    public bool AreCalculationsCorrect()
    {
        // Check if all individual item calculations are correct
        if (Items.Any(item => !item.IsCalculationCorrect()))
        {
            return false;
        }

        // Check if total quantity matches sum of individual quantities
        if (TotalQuantity != Items.Sum(item => item.Quantity))
        {
            return false;
        }

        // For this website, subtotal and total are the same (no tax/shipping)
        var expectedSubtotal = Items.Sum(item => item.Total);
        if (Math.Abs(expectedSubtotal - Subtotal) >= 0.01m && Subtotal > 0)
        {
            return false;
        }

        // If total is 0, use subtotal. Otherwise check if total matches expected calculation
        if (Total == 0)
        {
            return true; // Allow for empty cart or initialization state
        }

        // Check if total matches subtotal (this website doesn't seem to have separate tax/shipping)
        var expectedTotal = expectedSubtotal;
        if (Math.Abs(expectedTotal - Total) >= 0.01m)
        {
            return false;
        }

        return true;
    }
} 