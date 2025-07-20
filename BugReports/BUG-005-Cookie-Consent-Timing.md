# BUG-005: Cookie Consent Handling Timing Issue

## 🐛 **Bug Summary**
Cookie consent handler may execute before the consent dialog fully loads, causing it to be missed and leading to element click interception errors in subsequent test actions.

## 📊 **Bug Details**
- **Priority:** Medium 🟡
- **Severity:** Major
- **Category:** Timing/UI Interaction
- **Status:** Open
- **Reporter:** Automation Framework Analysis
- **Date Reported:** 2025-07-19

## 🔍 **Description**
The cookie consent handler has a fixed 5-second timeout and only executes once during initial navigation. This can cause consent dialogs to be missed if they load slowly, resulting in overlay issues that intercept subsequent element clicks throughout the test execution.

## 📁 **Framework Code Links**

### **Primary Location:**
```144:165:Tests/BaseTest.cs
protected async Task HandleCookieConsentAsync()
{
    try
    {
        var consentButton = await _page.WaitForSelectorAsync(".fc-button.fc-cta-consent", 
            new PageWaitForSelectorOptions { Timeout = 5000, State = WaitForSelectorState.Visible });
        
        if (consentButton != null)
        {
            await consentButton.ClickAsync();
            await Task.Delay(1000);
        }
    }
    catch (TimeoutException)
    {
        // Consent button not found, continue
    }
}
```

### **Usage in Navigation:**
```95:102:Tests/BaseTest.cs
protected async Task NavigateToAsync(string url)
{
    await _page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.Load });
    await HandleCookieConsentAsync();
    await WaitForPageLoadAsync();
}
```

### **Base Page Implementation:**
```47:62:Pages/BasePage.cs
protected async Task HandleCookieConsentAsync()
{
    try
    {
        var consentButton = await _page.QuerySelectorAsync(".fc-button.fc-cta-consent");
        if (consentButton != null)
        {
            await consentButton.ClickAsync();
            await Task.Delay(500);
            LogTestInfo("Cookie consent accepted");
        }
    }
    catch (Exception ex)
    {
        LogTestInfo($"Cookie consent handling: {ex.Message}");
    }
}
```

## 🎯 **Steps to Reproduce**
1. Run tests in an environment with slow network/loading
2. Navigate to any page with cookie consent dialog
3. Observe that consent dialog appears after the 5-second timeout
4. Subsequent clicks fail with "element click intercepted" errors
5. Notice inconsistent behavior between test runs

## ⚠️ **Expected vs Actual Behavior**

### **Expected:**
- Cookie consent dialog is detected and dismissed reliably
- No overlay interference with subsequent element interactions
- Consistent behavior across all test runs and environments
- Consent handling works regardless of loading speed

### **Actual:**
- 5-second timeout may be insufficient for slow environments
- Handler only executes once during initial navigation
- Consent dialog can appear after navigation completes
- Tests fail with click interception errors
- Inconsistent test results based on loading conditions

## 📈 **Impact on Tests**
- **Affected Tests:**
  - Any test that involves element clicking after navigation
  - Tests in slow network environments
  - Headless browser tests where timing differs
- **Common Error Messages:**
  - "Element click intercepted"
  - "Element is not clickable at point"
  - "Another element would receive the click"

## 🛠️ **Root Cause Analysis**
1. **Fixed Timeout Issue:** 5-second timeout insufficient for slow consent dialog loading
2. **Single Execution:** Handler only runs once during initial navigation
3. **No Retry Logic:** No mechanism to handle late-appearing dialogs
4. **Inconsistent Implementation:** Different logic in BaseTest vs BasePage
5. **No Verification:** No check that consent dialog actually disappeared
6. **Timing Assumption:** Assumes consent dialog loads within page load time

## 💡 **Recommended Fix**

### **Option 1: Retry-Based Consent Handling**
```csharp
protected async Task HandleCookieConsentAsync(int maxAttempts = 3)
{
    for (int attempt = 0; attempt < maxAttempts; attempt++)
    {
        try
        {
            // Check if consent dialog is visible
            var consentDialog = await _page.QuerySelectorAsync(".fc-dialog-container");
            if (consentDialog != null && await consentDialog.IsVisibleAsync())
            {
                var consentButton = await _page.WaitForSelectorAsync(".fc-button.fc-cta-consent", 
                    new PageWaitForSelectorOptions { Timeout = 3000, State = WaitForSelectorState.Visible });
                
                if (consentButton != null)
                {
                    await consentButton.ClickAsync();
                    
                    // Wait for dialog to disappear
                    await _page.WaitForSelectorAsync(".fc-dialog-container", 
                        new PageWaitForSelectorOptions { 
                            State = WaitForSelectorState.Hidden,
                            Timeout = 5000 
                        });
                    
                    LogTestInfo($"Cookie consent accepted on attempt {attempt + 1}");
                    return; // Success, exit retry loop
                }
            }
        }
        catch (TimeoutException)
        {
            if (attempt == maxAttempts - 1)
            {
                LogTestInfo("Cookie consent dialog not found after all attempts");
            }
        }
        
        // Wait before retry
        if (attempt < maxAttempts - 1)
        {
            await Task.Delay(2000);
        }
    }
}
```

### **Option 2: Proactive Consent Checking**
```csharp
protected async Task EnsureNoCookieConsentOverlayAsync()
{
    try
    {
        // Check for any visible consent dialog
        var consentDialog = await _page.QuerySelectorAsync(".fc-dialog-container");
        if (consentDialog != null && await consentDialog.IsVisibleAsync())
        {
            var consentButton = await _page.QuerySelectorAsync(".fc-button.fc-cta-consent");
            if (consentButton != null)
            {
                await consentButton.ClickAsync();
                await _page.WaitForSelectorAsync(".fc-dialog-container", 
                    new PageWaitForSelectorOptions { State = WaitForSelectorState.Hidden });
                LogTestInfo("Late-appearing cookie consent handled");
            }
        }
    }
    catch (Exception ex)
    {
        LogTestInfo($"Consent overlay check: {ex.Message}");
    }
}

// Call before any critical click operations
protected async Task ClickAsync(string selector)
{
    await EnsureNoCookieConsentOverlayAsync();
    await _page.ClickAsync(selector);
}
```

### **Option 3: Enhanced Detection with Multiple Selectors**
```csharp
protected async Task HandleCookieConsentAsync()
{
    var consentSelectors = new[]
    {
        ".fc-button.fc-cta-consent",           // Primary selector
        "[data-role='acceptAll']",             // Alternative selector
        ".cookie-consent-accept",              // Generic consent
        "#CybotCookiebotDialogBodyLevelButtonLevelOptinAllowAll" // Cookiebot
    };
    
    foreach (var selector in consentSelectors)
    {
        try
        {
            var consentButton = await _page.WaitForSelectorAsync(selector, 
                new PageWaitForSelectorOptions { Timeout = 2000, State = WaitForSelectorState.Visible });
            
            if (consentButton != null)
            {
                await consentButton.ClickAsync();
                
                // Wait for any modal/overlay to disappear
                await Task.Delay(1000);
                
                // Verify no consent dialog is visible
                var remainingDialogs = await _page.QuerySelectorAllAsync(".fc-dialog-container, .cookie-consent, [data-role='dialog']");
                var visibleDialogs = 0;
                foreach (var dialog in remainingDialogs)
                {
                    if (await dialog.IsVisibleAsync()) visibleDialogs++;
                }
                
                if (visibleDialogs == 0)
                {
                    LogTestInfo($"Cookie consent accepted using selector: {selector}");
                    return;
                }
            }
        }
        catch (TimeoutException)
        {
            // Try next selector
            continue;
        }
    }
    
    LogTestInfo("No cookie consent dialog found with any known selector");
}
```

## 🧪 **Test Case to Reproduce Bug**
```csharp
[Test]
public async Task ReproduceBug_CookieConsentTimingIssue()
{
    // Navigate with artificial delay to simulate slow loading
    await _page.GotoAsync(ConfigManager.BaseUrl, new PageGotoOptions { 
        WaitUntil = WaitUntilState.DOMContentLoaded // Don't wait for full load
    });
    
    // Immediately try to handle consent (will likely fail)
    await HandleCookieConsentAsync();
    
    // Simulate delay where consent dialog might appear
    await Task.Delay(6000);
    
    // Try to interact with an element - may fail with click interception
    try
    {
        await _page.ClickAsync(".nav-link[href='/products']");
        // Test should pass but may fail due to consent overlay
    }
    catch (PlaywrightException ex) when (ex.Message.Contains("intercepted"))
    {
        // This demonstrates the bug
        throw new Exception("Cookie consent overlay intercepted click - demonstrates BUG-005", ex);
    }
}
```

## 📝 **Current Implementation Issues**
- **Fixed Timeout:** 5-second timeout may be insufficient
- **Single Attempt:** No retry mechanism for late-appearing dialogs
- **No Verification:** Doesn't verify consent dialog actually disappeared
- **Inconsistent Logic:** Different implementations in BaseTest vs BasePage
- **Missing Error Context:** Generic timeout exceptions don't indicate consent issue

## 💡 **Enhanced Error Handling**
```csharp
public class CookieConsentException : Exception
{
    public CookieConsentException(string message) : base(message) { }
    public CookieConsentException(string message, Exception innerException) : base(message, innerException) { }
}

protected async Task HandleCookieConsentWithRetryAsync()
{
    var maxAttempts = 3;
    var exceptions = new List<Exception>();
    
    for (int attempt = 0; attempt < maxAttempts; attempt++)
    {
        try
        {
            if (await TryHandleConsentAsync())
            {
                return; // Success
            }
        }
        catch (Exception ex)
        {
            exceptions.Add(ex);
            if (attempt == maxAttempts - 1)
            {
                throw new CookieConsentException(
                    $"Failed to handle cookie consent after {maxAttempts} attempts", 
                    new AggregateException(exceptions));
            }
        }
        
        await Task.Delay(2000 * (attempt + 1)); // Exponential backoff
    }
}
```

## 🔗 **Related Issues**
- Similar overlay issues may exist with other modal dialogs
- Generic overlay detection could be improved framework-wide
- Page load completion detection needs enhancement

## 📋 **Acceptance Criteria for Fix**
- [ ] Cookie consent handling works reliably in slow environments
- [ ] No more "element click intercepted" errors due to consent overlays
- [ ] Retry mechanism for late-appearing consent dialogs
- [ ] Verification that consent dialog actually disappears
- [ ] Consistent behavior across all test environments
- [ ] Enhanced error messages for consent-related failures

---
**Last Updated:** 2025-07-19
**Framework Version:** 2.3.0 