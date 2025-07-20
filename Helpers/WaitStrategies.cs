using Microsoft.Playwright;

namespace AutomationExercise.Tests.Helpers
{
    /// <summary>
    /// Centralized wait strategies following DRY principle
    /// Provides reusable, configurable wait methods for different scenarios
    /// </summary>
    public static class WaitStrategies
    {
        #region Constants
        private static readonly TimeoutSettings _timeouts = ConfigManager.TestConfig.Timeouts;
        private static readonly int DefaultTimeoutMs = _timeouts.Default;
        private static readonly int ShortTimeoutMs = _timeouts.Short;
        private static readonly int NetworkIdleTimeoutMs = _timeouts.NetworkIdle;
        private static readonly int FallbackDelayMs = 2000;
        #endregion

        #region Element-Based Waits
        
        /// <summary>
        /// Wait for element to be visible with progressive fallback strategy
        /// </summary>
        public static async Task<bool> WaitForElementVisibleAsync(IPage page, string selector)
        {
            return await WaitForElementVisibleAsync(page, selector, DefaultTimeoutMs);
        }
        
        /// <summary>
        /// Wait for element to be visible with progressive fallback strategy
        /// </summary>
        public static async Task<bool> WaitForElementVisibleAsync(
            IPage page, 
            string selector, 
            int timeoutMs)
        {
            try
            {
                await page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = timeoutMs
                });
                return true;
            }
            catch (TimeoutException)
            {
                return false;
            }
        }

        /// <summary>
        /// Wait for multiple elements to be present
        /// </summary>
        public static async Task<bool> WaitForElementsAsync(
            IPage page, 
            string selector, 
            int minimumCount = 1, 
            int timeoutMs = 30000)
        {
            try
            {
                await page.WaitForFunctionAsync($@"
                    () => document.querySelectorAll('{selector}').length >= {minimumCount}
                ", new PageWaitForFunctionOptions { Timeout = timeoutMs });
                return true;
            }
            catch (TimeoutException)
            {
                return false;
            }
        }

        /// <summary>
        /// Wait for element to disappear (useful for loading spinners)
        /// </summary>
        public static async Task<bool> WaitForElementToDisappearAsync(
            IPage page, 
            string selector, 
            int timeoutMs = 30000)
        {
            try
            {
                await page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions
                {
                    State = WaitForSelectorState.Hidden,
                    Timeout = timeoutMs
                });
                return true;
            }
            catch (TimeoutException)
            {
                return false;
            }
        }
        #endregion

        #region Page State Waits

        /// <summary>
        /// Progressive wait strategy for page loads with multiple fallback options
        /// Implements smart waiting based on page behavior
        /// </summary>
        public static async Task WaitForPageReadyAsync(IPage page)
        {
            await ExecuteProgressiveWaitStrategy(page, new WaitStrategy[]
            {
                new WaitStrategy("DOMContentLoaded", () => 
                    page.WaitForLoadStateAsync(LoadState.DOMContentLoaded, 
                        new PageWaitForLoadStateOptions { Timeout = ShortTimeoutMs })),
                
                new WaitStrategy("NetworkIdle", () => 
                    page.WaitForLoadStateAsync(LoadState.NetworkIdle, 
                        new PageWaitForLoadStateOptions { Timeout = NetworkIdleTimeoutMs })),
                
                new WaitStrategy("Fallback delay", () => Task.Delay(FallbackDelayMs))
            });
        }

        /// <summary>
        /// Wait for search results to load with intelligent detection
        /// </summary>
        public static async Task WaitForSearchResultsAsync(IPage page, string productSelector)
        {
            await ExecuteProgressiveWaitStrategy(page, new WaitStrategy[]
            {
                new WaitStrategy("Product elements visible", () => 
                    WaitForElementVisibleAsync(page, productSelector, ShortTimeoutMs).ContinueWith(t => { })),
                
                new WaitStrategy("Network idle", () => 
                    page.WaitForLoadStateAsync(LoadState.NetworkIdle, 
                        new PageWaitForLoadStateOptions { Timeout = NetworkIdleTimeoutMs })),
                
                new WaitStrategy("Fallback delay", () => Task.Delay(FallbackDelayMs))
            });
        }

        /// <summary>
        /// Wait for URL to change (useful for navigation verification)
        /// </summary>
        public static async Task<bool> WaitForUrlChangeAsync(
            IPage page, 
            string expectedUrlPattern, 
            int timeoutMs = 30000)
        {
            try
            {
                await page.WaitForURLAsync(expectedUrlPattern, new PageWaitForURLOptions
                {
                    Timeout = timeoutMs
                });
                return true;
            }
            catch (TimeoutException)
            {
                return false;
            }
        }
        #endregion

        #region Custom Conditions

        /// <summary>
        /// Wait for custom condition with polling
        /// </summary>
        public static async Task<T> WaitForConditionAsync<T>(
            Func<Task<T>> condition, 
            Func<T, bool> predicate, 
            int timeoutMs = 30000, 
            int pollingIntervalMs = 500)
        {
            var endTime = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            
            while (DateTime.UtcNow < endTime)
            {
                try
                {
                    var result = await condition();
                    if (predicate(result))
                        return result;
                }
                catch
                {
                    // Continue polling on exceptions
                }
                
                await Task.Delay(pollingIntervalMs);
            }
            
            throw new TimeoutException($"Condition not met within {timeoutMs}ms");
        }

        /// <summary>
        /// Wait for text content to match expectation
        /// </summary>
        public static async Task<bool> WaitForTextContentAsync(
            IPage page, 
            string selector, 
            string expectedText, 
            bool exactMatch = false, 
            int timeoutMs = 30000)
        {
            try
            {
                var comparison = exactMatch ? 
                    $"element => element.textContent.trim() === '{expectedText}'" :
                    $"element => element.textContent.toLowerCase().includes('{expectedText.ToLower()}')";

                await page.WaitForFunctionAsync($@"
                    () => {{
                        const element = document.querySelector('{selector}');
                        return element && ({comparison});
                    }}
                ", new PageWaitForFunctionOptions { Timeout = timeoutMs });
                return true;
            }
            catch (TimeoutException)
            {
                return false;
            }
        }
        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Execute progressive wait strategy with multiple fallbacks
        /// </summary>
        private static async Task ExecuteProgressiveWaitStrategy(IPage page, WaitStrategy[] strategies)
        {
            foreach (var strategy in strategies)
            {
                try
                {
                    await strategy.ExecuteAsync();
                    return; // Success, no need to continue
                }
                catch (TimeoutException)
                {
                    // Continue to next strategy
                    continue;
                }
            }
        }

        /// <summary>
        /// Internal wait strategy representation
        /// </summary>
        private class WaitStrategy
        {
            public string Name { get; }
            public Func<Task> ExecuteAsync { get; }

            public WaitStrategy(string name, Func<Task> executeAsync)
            {
                Name = name;
                ExecuteAsync = executeAsync;
            }
        }
        #endregion
    }
} 