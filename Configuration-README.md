# 🔧 Configuration Management System

This framework now uses a centralized configuration system to eliminate hardcoding and improve maintainability.

## ✅ **What Was Fixed**

### **Before (Hardcoded):**
- URLs hardcoded in multiple files
- Selectors scattered across page objects
- Timeout values duplicated everywhere
- Test data embedded in code
- Currency/locale hardcoded

### **After (Configured):**
- All settings in `appsettings.json`
- Environment-specific overrides
- External test data files
- Centralized constants
- Easy to maintain and update

## 📁 **Configuration Structure**

### **1. Main Configuration (`appsettings.json`)**
```json
{
  "TestSettings": {
    "BaseUrl": "https://automationexercise.com",
    "Timeouts": { ... },
    "Retry": { ... },
    "Localization": { ... },
    "Paths": { ... }
  },
  "Selectors": {
    "HomePage": { ... },
    "ProductsPage": { ... },
    "CartPage": { ... }
  }
}
```

### **2. Environment Overrides**
- `appsettings.staging.json` - Staging environment
- `appsettings.production.json` - Production environment
- Set `TEST_ENV` environment variable to use

### **3. External Test Data**
- `TestData/SearchTestData.json` - Search test parameters
- `TestData/ProductTestData.json` - Product test data
- Easy to update without code changes

### **4. Constants (`TestConstants.cs`)**
- Expected counts
- Default values
- Browser types
- Categories
- Delays

## 🚀 **Usage Examples**

### **Accessing Configuration in Code:**
```csharp
// Get base URL
var baseUrl = ConfigManager.TestConfig.BaseUrl;

// Get timeout
var timeout = ConfigManager.TestConfig.Timeouts.Default;

// Get selector
var searchInput = ConfigManager.Selectors.ProductsPage.Search.SearchInput;

// Get currency
var currency = ConfigManager.TestConfig.Localization.Currency;
```

### **Using Different Environments:**
```bash
# Use staging configuration
$env:TEST_ENV="staging"
dotnet test

# Use production configuration
$env:TEST_ENV="production"
dotnet test
```

### **Overriding via Command Line:**
```powershell
# Override base URL
.\RunTestsWithReport.ps1 -BaseUrl "https://test.site.com"

# Override timeout
.\RunTestsWithReport.ps1 -Timeout 60000
```

## 📋 **Configuration Hierarchy**

1. **Command line parameters** (highest priority)
2. **Environment variables**
3. **Environment-specific config** (appsettings.{env}.json)
4. **Base configuration** (appsettings.json)
5. **Default values in code** (lowest priority)

## 🔄 **Migration Guide**

### **For Page Objects:**
```csharp
// Old (hardcoded)
private readonly string _searchInput = "#search_product";

// New (configured)
private readonly string _searchInput;
constructor() {
    _searchInput = ConfigManager.Selectors.ProductsPage.Search.SearchInput;
}
```

### **For Timeouts:**
```csharp
// Old (hardcoded)
const int timeout = 30000;

// New (configured)
var timeout = ConfigManager.TestConfig.Timeouts.Default;
```

### **For Test Data:**
```csharp
// Old (hardcoded)
yield return new object[] { "Top", 14 };

// New (from file)
var testData = LoadTestData<SearchTestData>("SearchTestData.json");
```

## 🎯 **Benefits**

1. **No more hardcoding** - All values externalized
2. **Environment flexibility** - Easy to test different environments
3. **Maintainability** - Change values without touching code
4. **Consistency** - Single source of truth
5. **Testability** - Easy to mock configurations
6. **Scalability** - Add new environments easily

## 📊 **What's Configurable Now**

- ✅ Base URLs
- ✅ All CSS selectors
- ✅ Timeout values
- ✅ Retry settings
- ✅ Currency/locale
- ✅ File paths
- ✅ Test data
- ✅ Browser settings

## 🔮 **Future Enhancements**

1. **Remote configuration** - Load from API/database
2. **Hot reload** - Change config without restart
3. **Validation** - Ensure all required settings present
4. **Encryption** - For sensitive values
5. **A/B testing** - Multiple config variants

---

**Configuration Version:** 1.0
**Last Updated:** 2025-07-19 