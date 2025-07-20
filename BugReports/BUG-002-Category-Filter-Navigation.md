# BUG-002: Category Filter Navigation Not Working as Expected

## 🐛 **Bug Summary**
Category filter implementation incorrectly targets accordion toggles instead of actual category filter links, resulting in no actual product filtering.

## 📊 **Bug Details**
- **Priority:** High 🔴
- **Severity:** Major
- **Category:** Logic/Functional
- **Status:** Open
- **Reporter:** Automation Framework Analysis
- **Date Reported:** 2025-07-19

## 🔍 **Description**
The category filter functionality assumes categories expand/collapse properly and filter products, but the current implementation only expands accordion sections without actually filtering products. All category tests show 34 products regardless of the selected category.

## 📁 **Framework Code Links**

### **Primary Location:**
```167:177:Pages/ProductsPage.cs
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
```

### **Selector Definitions:**
```27:29:Pages/ProductsPage.cs
private readonly string _womenCategory = "a[href='#Women']";
private readonly string _menCategory = "a[href='#Men']";
private readonly string _kidsCategory = "a[href='#Kids']";
```

### **Test Implementation:**
```131:145:Tests/ProductSearchTests.cs
[TestCaseSource(nameof(CategoryTestData))]
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
}
```

## 🎯 **Steps to Reproduce**
1. Navigate to Products page
2. Call `FilterByCategoryAsync("Women")`
3. Verify product count - shows 34 products (all products)
4. Repeat for Men and Kids categories
5. Observe that no actual filtering occurs

## ⚠️ **Expected vs Actual Behavior**

### **Expected:**
- Clicking a category should filter products to that category only
- Product count should be reduced to category-specific items
- Only relevant products should be displayed

### **Actual:**
- Category accordion expands but no filtering occurs
- All 34 products remain visible
- Test passes with false positive results

## 📈 **Impact on Tests**
- **Affected Tests:**
  - `FilterProducts_ByCategory_ShouldShowCategoryProducts("Women")`
  - `FilterProducts_ByCategory_ShouldShowCategoryProducts("Men")`
  - `FilterProducts_ByCategory_ShouldShowCategoryProducts("Kids")`

## 🛠️ **Root Cause Analysis**
1. Selectors `a[href='#Women']` point to Bootstrap accordion toggles
2. These toggles expand/collapse category sections but don't filter products
3. Actual category filtering requires clicking on subcategory links
4. Framework doesn't verify that filtering actually occurred
5. No validation that product count changed after filtering

## 💡 **Recommended Fix**

### **Option 1: Two-Step Category Navigation**
```csharp
public async Task FilterByCategoryAsync(string categoryName)
{
    // Step 1: Expand the category accordion
    var categoryToggle = $"a[href='#{categoryName}']";
    await ClickAsync(categoryToggle);
    await Task.Delay(500); // Wait for accordion animation
    
    // Step 2: Click on the first subcategory to actually filter
    var subcategorySelector = $"#{categoryName} + .panel-body a:first-child";
    await ClickAsync(subcategorySelector);
    await WaitForPageLoadAsync();
    
    // Step 3: Verify filtering occurred
    await VerifyFilteringAppliedAsync();
}

private async Task VerifyFilteringAppliedAsync()
{
    // Wait for URL to change or filtered results to load
    await _page.WaitForURLAsync(url => url.Contains("category") || url.Contains("subcategory"));
    await WaitForElementAsync(_productItems);
}
```

### **Option 2: Direct Subcategory Navigation**
```csharp
public async Task FilterByCategoryAsync(string categoryName, string subcategory = null)
{
    var categoryPaths = new Dictionary<string, string[]>
    {
        ["women"] = new[] { "dress", "tops", "saree" },
        ["men"] = new[] { "tshirts", "jeans" },
        ["kids"] = new[] { "dress", "tops-shirts" }
    };

    if (categoryPaths.TryGetValue(categoryName.ToLower(), out var subcategories))
    {
        var targetSubcategory = subcategory ?? subcategories[0];
        var categoryUrl = $"{_baseUrl}/category_products/{GetCategoryId(categoryName, targetSubcategory)}";
        await NavigateToAsync(categoryUrl);
        await WaitForPageLoadAsync();
    }
}
```

## 🧪 **Test Case to Reproduce Bug**
```csharp
[Test]
public async Task ReproduceBug_CategoryFilterNotWorking()
{
    await NavigateToProductsPageAsync();
    var initialCount = await _productsPage!.GetProductCountAsync();
    
    // Try filtering by Women category
    await _productsPage.FilterByCategoryAsync("Women");
    var filteredCount = await _productsPage.GetProductCountAsync();
    
    // This assertion should fail, proving the bug
    filteredCount.Should().BeLessThan(initialCount, 
        "Filtering should reduce the number of products shown");
}
```

## 📝 **Current Test Results**
```
FilterProducts_ByCategory_ShouldShowCategoryProducts("Women") ✅ PASSED (4s) - Found 34 products
FilterProducts_ByCategory_ShouldShowCategoryProducts("Men") ✅ PASSED (4s) - Found 34 products  
FilterProducts_ByCategory_ShouldShowCategoryProducts("Kids") ✅ PASSED (4s) - Found 34 products
```

**Problem:** All tests pass but show identical product counts, indicating no filtering occurred.

## 💡 **Enhanced Test Validation**
```csharp
[Test]
public async Task FilterProducts_ByCategory_ShouldActuallyFilter(string category)
{
    await NavigateToProductsPageAsync();
    var allProductsCount = await _productsPage!.GetProductCountAsync();
    
    await _productsPage.FilterByCategoryAsync(category);
    
    var filteredProductsCount = await _productsPage.GetProductCountAsync();
    var products = await _productsPage.GetAllProductsAsync();
    
    // Enhanced assertions
    filteredProductsCount.Should().BeLessThan(allProductsCount, 
        "Category filtering should reduce product count");
    
    products.Should().AllSatisfy(product => 
        product.Category.Should().Contain(category, StringComparison.OrdinalIgnoreCase),
        "All displayed products should belong to the selected category");
}
```

## 🔗 **Related Issues**
- Brand filtering may have similar implementation issues
- Generic filtering validation needs improvement framework-wide
- Test data factory provides incorrect expected results

## 📋 **Acceptance Criteria for Fix**
- [ ] Category filtering actually reduces the product count
- [ ] Only category-relevant products are displayed after filtering
- [ ] URL changes to reflect the selected category/subcategory
- [ ] Tests fail when filtering doesn't work properly
- [ ] Enhanced validation prevents false positives

---
**Last Updated:** 2025-07-19
**Framework Version:** 2.3.0 