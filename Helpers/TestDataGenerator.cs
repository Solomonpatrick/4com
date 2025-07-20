using Bogus;
using AutomationExercise.Tests.Models;

namespace AutomationExercise.Tests.Helpers;

/// <summary>
/// Generates test data for automation tests
/// </summary>
public static class TestDataGenerator
{
    private static readonly Faker _faker = new();

    /// <summary>
    /// Common search terms used in the application
    /// </summary>
    public static readonly string[] SearchTerms = 
    {
        "Top",
        "Dress",
        "Shirt", 
        "Jeans",
        "Tshirt",
        "Saree",
        "Kids",
        "Women",
        "Men",
        "Blue",
        "Pink",
        "Cotton",
        "Polo"
    };

    /// <summary>
    /// Product categories available in the application
    /// </summary>
    public static readonly string[] Categories = 
    {
        "Women",
        "Men", 
        "Kids"
    };

    /// <summary>
    /// Brand names available in the application
    /// </summary>
    public static readonly string[] Brands = 
    {
        "Polo",
        "H&M",
        "Madame",
        "Mast & Harbour",
        "Babyhug",
        "Allen Solly Junior",
        "Kookie Kids",
        "Biba"
    };

    /// <summary>
    /// Generate a random search term
    /// </summary>
    public static string GetRandomSearchTerm()
    {
        return _faker.PickRandom(SearchTerms);
    }

    /// <summary>
    /// Generate multiple random search terms
    /// </summary>
    public static IEnumerable<string> GetRandomSearchTerms(int count)
    {
        return _faker.PickRandom(SearchTerms, count);
    }

    /// <summary>
    /// Generate a random category
    /// </summary>
    public static string GetRandomCategory()
    {
        return _faker.PickRandom(Categories);
    }

    /// <summary>
    /// Generate a random brand
    /// </summary>
    public static string GetRandomBrand()
    {
        return _faker.PickRandom(Brands);
    }

    /// <summary>
    /// Generate a fake product for testing
    /// </summary>
    public static Product GenerateFakeProduct()
    {
        return new Product
        {
            Name = _faker.Commerce.ProductName(),
            Price = decimal.Parse(_faker.Commerce.Price()),
            Description = _faker.Commerce.ProductDescription(),
            Category = GetRandomCategory(),
            Brand = GetRandomBrand(),
            ImageUrl = _faker.Image.PicsumUrl(),
            IsAvailable = _faker.Random.Bool(0.9f), // 90% chance of being available
            Id = _faker.Random.AlphaNumeric(8)
        };
    }

    /// <summary>
    /// Generate multiple fake products
    /// </summary>
    public static IEnumerable<Product> GenerateFakeProducts(int count)
    {
        var products = new List<Product>();
        for (int i = 0; i < count; i++)
        {
            var product = GenerateFakeProduct();
            product.Index = i;
            products.Add(product);
        }
        return products;
    }

    /// <summary>
    /// Generate a fake cart item
    /// </summary>
    public static CartItem GenerateFakeCartItem()
    {
        var price = decimal.Parse(_faker.Commerce.Price());
        var quantity = _faker.Random.Int(1, 5);
        
        return new CartItem
        {
            Name = _faker.Commerce.ProductName(),
            Price = price,
            Quantity = quantity,
            Total = price * quantity,
            ImageUrl = _faker.Image.PicsumUrl(),
            ProductId = _faker.Random.AlphaNumeric(8)
        };
    }

    /// <summary>
    /// Generate multiple fake cart items
    /// </summary>
    public static IEnumerable<CartItem> GenerateFakeCartItems(int count)
    {
        var items = new List<CartItem>();
        for (int i = 0; i < count; i++)
        {
            var item = GenerateFakeCartItem();
            item.Index = i;
            items.Add(item);
        }
        return items;
    }

    /// <summary>
    /// Generate test data for parametrized tests
    /// </summary>
    public static object[][] GenerateSearchTestData()
    {
        return SearchTerms.Select(term => new object[] { term }).ToArray();
    }

    /// <summary>
    /// Generate test data for category filtering tests
    /// </summary>
    public static object[][] GenerateCategoryTestData()
    {
        return Categories.Select(category => new object[] { category }).ToArray();
    }

    /// <summary>
    /// Generate test data for brand filtering tests
    /// </summary>
    public static object[][] GenerateBrandTestData()
    {
        return Brands.Select(brand => new object[] { brand }).ToArray();
    }

    /// <summary>
    /// Generate random test combinations for comprehensive testing
    /// </summary>
    public static IEnumerable<(string searchTerm, string category, string brand)> GenerateTestCombinations(int count = 10)
    {
        for (int i = 0; i < count; i++)
        {
            yield return (
                GetRandomSearchTerm(),
                GetRandomCategory(),
                GetRandomBrand()
            );
        }
    }
} 