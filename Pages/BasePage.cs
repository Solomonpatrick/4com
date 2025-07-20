using Microsoft.Playwright;
using AutomationExercise.Tests.Helpers;

namespace AutomationExercise.Tests.Pages;

/// <summary>
/// Base page class containing common functionality for all page objects
/// </summary>
public abstract class BasePage
{
    protected readonly IPage _page;
    protected readonly string _baseUrl;
    protected readonly TestConfiguration _config;
    protected readonly CommonSelectors _commonSelectors;

    protected BasePage(IPage page)
    {
        _page = page;
        _config = ConfigManager.TestConfig;
        _baseUrl = _config.BaseUrl;
        _commonSelectors = ConfigManager.Selectors.Common;
    }

    /// <summary>
    /// Navigate to a specific URL
    /// </summary>
    protected async Task NavigateToAsync(string url)
    {
        await _page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.Load });
    }

    /// <summary>
    /// Wait for an element to be visible
    /// </summary>
    protected async Task WaitForElementAsync(string selector, int timeout = 30000)
    {
        await _page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions 
        { 
            State = WaitForSelectorState.Visible,
            Timeout = timeout 
        });
    }

    /// <summary>
    /// Click an element with retry mechanism
    /// </summary>
    protected async Task ClickAsync(string selector, int retries = 3)
    {
        for (int i = 0; i < retries; i++)
        {
            try
            {
                // Wait for element and scroll into view
                await WaitForElementAsync(selector);
                await ScrollIntoViewAsync(selector);
                await Task.Delay(500); // Wait for any animations to complete
                
                // Try normal click first
                await _page.ClickAsync(selector, new PageClickOptions { Timeout = 5000 });
                return;
            }
            catch (TimeoutException) when (i < retries - 1)
            {
                // If normal click fails, try force click on final retry
                if (i == retries - 2)
                {
                    try
                    {
                        await _page.ClickAsync(selector, new PageClickOptions { Force = true, Timeout = 5000 });
                        return;
                    }
                    catch
                    {
                        // Continue to final retry
                    }
                }
                await Task.Delay(1000);
            }
        }
        throw new TimeoutException($"Failed to click element '{selector}' after {retries} retries");
    }

    /// <summary>
    /// Fill text in an input field
    /// </summary>
    protected async Task FillAsync(string selector, string text)
    {
        await _page.FillAsync(selector, text);
    }

    /// <summary>
    /// Get text content of an element
    /// </summary>
    protected async Task<string> GetTextAsync(string selector)
    {
        return await _page.TextContentAsync(selector) ?? string.Empty;
    }

    /// <summary>
    /// Get all text contents matching a selector
    /// </summary>
    protected async Task<IReadOnlyList<string>> GetAllTextsAsync(string selector)
    {
        var elements = await _page.QuerySelectorAllAsync(selector);
        var texts = new List<string>();
        
        foreach (var element in elements)
        {
            var text = await element.TextContentAsync();
            if (!string.IsNullOrEmpty(text))
            {
                texts.Add(text);
            }
        }
        
        return texts;
    }

    /// <summary>
    /// Check if an element is visible
    /// </summary>
    protected async Task<bool> IsVisibleAsync(string selector)
    {
        try
        {
            return await _page.IsVisibleAsync(selector);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Wait for page to load completely using progressive wait strategy
    /// </summary>
    protected async Task WaitForPageLoadAsync()
    {
        await WaitStrategies.WaitForPageReadyAsync(_page);
    }

    /// <summary>
    /// Scroll element into view
    /// </summary>
    protected async Task ScrollIntoViewAsync(string selector)
    {
        await _page.EvaluateAsync("selector => { const element = document.querySelector(selector); if (element) element.scrollIntoView({ behavior: 'smooth', block: 'center' }); }", selector);
    }

    /// <summary>
    /// Handle cookie consent popup
    /// </summary>
    protected async Task HandleCookieConsentAsync()
    {
        try
        {
            // Wait for consent dialog and click consent
            try
            {
                var consentButton = await _page.WaitForSelectorAsync(".fc-button.fc-cta-consent", new PageWaitForSelectorOptions 
                { 
                    Timeout = 5000,
                    State = WaitForSelectorState.Visible 
                });
                
                if (consentButton != null)
                {
                    await consentButton.ClickAsync();
                    await Task.Delay(1000); // Wait for consent to be processed
                }
            }
            catch (TimeoutException)
            {
                // Consent button not found, continue
            }
        }
        catch (TimeoutException)
        {
            // No consent dialog found, continue
        }
    }

    /// <summary>
    /// Take a screenshot for debugging
    /// </summary>
    protected async Task TakeScreenshotAsync(string name)
    {
        await _page.ScreenshotAsync(new PageScreenshotOptions 
        { 
            Path = $"screenshots/{name}_{DateTime.Now:yyyyMMdd_HHmmss}.png",
            FullPage = true 
        });
    }
} 