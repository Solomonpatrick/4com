namespace AutomationExercise.Tests.Helpers;

/// <summary>
/// Centralized constants for test framework
/// </summary>
public static class TestConstants
{
    /// <summary>
    /// Expected product counts for different test scenarios
    /// </summary>
    public static class ExpectedCounts
    {
        public const int TopSearchResults = 14;
        public const int DressSearchResults = 9;
        public const int ShirtSearchResults = 13;
        public const int JeansSearchResults = 3;
        public const int TshirtSearchResults = 6;
        public const int SareeSearchResults = 3;
    }
    
    /// <summary>
    /// Test data constants
    /// </summary>
    public static class TestData
    {
        public const string DefaultProductName = "Blue Top";
        public const string DefaultCategory = "Women > Tops";
        public const string DefaultBrand = "Polo";
        public const decimal DefaultPrice = 500;
    }
    
    /// <summary>
    /// Browser types
    /// </summary>
    public static class BrowserTypes
    {
        public const string Chromium = "chromium";
        public const string Firefox = "firefox";
        public const string Webkit = "webkit";
    }
    
    /// <summary>
    /// Category names
    /// </summary>
    public static class Categories
    {
        public const string Women = "Women";
        public const string Men = "Men";
        public const string Kids = "Kids";
    }
    
    /// <summary>
    /// Default delays for synchronization
    /// </summary>
    public static class Delays
    {
        public const int FallbackDelay = 2000;
        public const int SearchResultsDelay = 1000;
        public const int ModalAnimationDelay = 500;
        public const int ElementInteractionDelay = 100;
    }
} 