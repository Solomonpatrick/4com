# BUG-001: Race Condition in Cart Modal Handling

## 🐛 **Bug Summary**
Cart modal interactions fail intermittently due to race condition between modal appearance and content readiness.

## 📊 **Bug Details**
- **Priority:** High 🔴
- **Severity:** Major
- **Category:** Timing/Synchronization
- **Status:** Open
- **Reporter:** Automation Framework Analysis
- **Date Reported:** 2025-07-19

## 🔍 **Description**
The cart modal may not always be fully rendered when tests attempt to interact with it, causing intermittent failures. The framework waits for the modal visibility but doesn't verify that the modal content is fully loaded or interactive.

## 📁 **Framework Code Links**

### **Primary Location:**
```230:235:Pages/ProductsPage.cs
// Click the button using JavaScript to bypass any overlay issues
await addToCartButtons[productIndex].EvaluateAsync("element => element.click()");

// Wait for the cart modal to become visible
await _page.WaitForSelectorAsync(_cartModal + ".show", new PageWaitForSelectorOptions { Timeout = 10000 });
```

### **Secondary Locations:**
```164:169:Pages/ProductsPage.cs
public async Task ViewCartFromModalAsync()
{
    await ClickAsync(_viewCartButton);
    await WaitForPageLoadAsync();
    return new CartPage(_page);
}
```

```73:86:Pages/HomePage.cs
// Wait for the cart modal to become visible
await _page.WaitForSelectorAsync(_cartModal + ".show", new PageWaitForSelectorOptions { Timeout = 10000 });
```

## 🎯 **Steps to Reproduce**
1. Navigate to Products page
2. Click "Add to Cart" on any product
3. Immediately try to interact with modal buttons
4. Observe intermittent failures in headless mode or slow environments

## ⚠️ **Expected vs Actual Behavior**

### **Expected:**
- Modal appears and is immediately interactive
- All modal buttons work reliably
- Tests pass consistently

### **Actual:**
- Modal appears but content may not be ready
- Click interactions may fail silently
- Tests fail intermittently with timing issues

## 📈 **Impact on Tests**
- **Affected Tests:**
  - `AddProductToCartAsync()` in ProductsPage
  - `AddSingleProductToCart_ShouldAddSuccessfully`
  - `AddMultipleProductsToCart_ShouldAddAllSuccessfully`
  - Any test using cart modal functionality

## 🛠️ **Root Cause Analysis**
1. Framework waits for CSS class `.show` to appear
2. Modal animation may still be in progress
3. Modal content (buttons) may not be interactive yet
4. No verification that modal is ready for interaction

## 💡 **Recommended Fix**

### **Option 1: Enhanced Wait Strategy**
```csharp
public async Task AddProductToCartAsync(int productIndex)
{
    // ... existing code ...
    await addToCartButtons[productIndex].EvaluateAsync("element => element.click()");
    
    // Wait for modal to appear AND be interactive
    await _page.WaitForSelectorAsync(_cartModal + ".show", new PageWaitForSelectorOptions { Timeout = 10000 });
    await _page.WaitForSelectorAsync(_viewCartButton, new PageWaitForSelectorOptions { 
        State = WaitForSelectorState.Visible,
        Timeout = 5000 
    });
    
    // Ensure modal animation completes
    await Task.Delay(500);
}
```

### **Option 2: Modal Ready State Check**
```csharp
private async Task WaitForModalReadyAsync()
{
    // Wait for modal visibility
    await _page.WaitForSelectorAsync(_cartModal + ".show");
    
    // Wait for all modal buttons to be interactive
    await _page.WaitForFunctionAsync(@"
        const modal = document.querySelector('#cartModal.show');
        const viewCartBtn = modal?.querySelector('a[href=""/view_cart""]');
        const continueBtn = modal?.querySelector('.close-modal');
        
        return modal && viewCartBtn && continueBtn && 
               !modal.classList.contains('fade') &&
               getComputedStyle(modal).opacity === '1';
    ");
}
```

## 🧪 **Test Case to Reproduce**
```csharp
[Test]
public async Task ReproduceBug_CartModalRaceCondition()
{
    await NavigateToProductsPageAsync();
    
    // Rapid-fire add to cart operations
    for (int i = 0; i < 5; i++)
    {
        await _productsPage!.AddProductToCartAsync(0);
        await _productsPage.ContinueShoppingAsync();
        await Task.Delay(100); // Minimal delay to stress test
    }
}
```

## 📝 **Workaround**
Current workaround: Add manual delays after modal appears:
```csharp
await _page.WaitForSelectorAsync(_cartModal + ".show");
await Task.Delay(1000); // Allow modal to fully render
```

## 🔗 **Related Issues**
- Similar timing issues may exist in other modal interactions
- Generic modal handling could be improved framework-wide

## 📋 **Acceptance Criteria for Fix**
- [ ] Modal interactions work 100% reliably in headless mode
- [ ] No more intermittent failures in cart-related tests
- [ ] Modal ready state is properly verified before interaction
- [ ] Fix is applied to all modal interaction methods

---
**Last Updated:** 2025-07-19
**Framework Version:** 2.3.0 