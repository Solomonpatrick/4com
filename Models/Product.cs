namespace AutomationExercise.Tests.Models;

/// <summary>
/// Represents a product in the automation exercise application
/// </summary>
public class Product
{
    /// <summary>
    /// Product name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Product price (numeric value without currency)
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Product image URL
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Product index in the list (for UI interaction)
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Product description (if available)
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Product category
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Product brand
    /// </summary>
    public string Brand { get; set; } = string.Empty;

    /// <summary>
    /// Product availability status
    /// </summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// Product ID (if available)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Returns a string representation of the product
    /// </summary>
    public override string ToString()
    {
        return $"{Name} - Rs. {Price}";
    }

    /// <summary>
    /// Check if product matches a search term
    /// </summary>
    public bool MatchesSearchTerm(string searchTerm)
    {
        return Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
               Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
               Category.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
               Brand.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
    }
} 