using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports.Reporter.Config;
using System.Reflection;

namespace AutomationExercise.Tests.Helpers;

/// <summary>
/// Manages test reporting with ExtentReports
/// </summary>
public class ReportManager
{
    private static ExtentReports? _extent;
    private static string _reportDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestReports");
    private static string? _timestamp;
    private static string? _reportPath;
    
    /// <summary>
    /// Initialize the ExtentReports instance
    /// </summary>
    public static void InitializeReport()
    {
        // Skip if already initialized
        if (_extent != null)
        {
            Console.WriteLine("⚠️ Report already initialized, skipping...");
            return;
        }
        
        // Generate fresh timestamp for this test run
        _timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        
        // Create TestReports directory in the project root (not in bin folder)
        var projectRoot = Directory.GetCurrentDirectory();
        // If we're running from bin folder, go back to project root
        if (projectRoot.Contains("bin"))
        {
            projectRoot = Path.GetFullPath(Path.Combine(projectRoot, "..", "..", ".."));
        }
        
        _reportDirectory = Path.Combine(projectRoot, "TestReports");
        _reportPath = Path.Combine(_reportDirectory, $"TestReport_{_timestamp}.html");
        
        // Ensure report directory exists
        Directory.CreateDirectory(_reportDirectory);
        
        // Delete all previous reports
        DeleteAllReports();
        
        // Initialize ExtentHtmlReporter
        var htmlReporter = new ExtentSparkReporter(_reportPath);
        
        // Configure the HTML reporter
        htmlReporter.Config.Theme = Theme.Standard;
        htmlReporter.Config.DocumentTitle = "AutomationExercise Test Report";
        htmlReporter.Config.ReportName = "Automation Test Results";
        htmlReporter.Config.Encoding = "UTF-8";
        
        // Initialize ExtentReports
        _extent = new ExtentReports();
        _extent.AttachReporter(htmlReporter);
        
        // Add system information
        AddSystemInformation();
        
        Console.WriteLine($"📊 Test report will be generated at: {_reportPath}");
    }
    
    /// <summary>
    /// Add system information to the report
    /// </summary>
    private static void AddSystemInformation()
    {
        if (_extent == null) return;
        
        _extent.AddSystemInfo("Application", "AutomationExercise.com");
        _extent.AddSystemInfo("Base URL", "https://automationexercise.com");
        _extent.AddSystemInfo("Test Framework", "NUnit + Playwright");
        _extent.AddSystemInfo("Language", "C# (.NET 8.0)");
        _extent.AddSystemInfo("OS", Environment.OSVersion.ToString());
        _extent.AddSystemInfo("User", Environment.UserName);
        _extent.AddSystemInfo("Machine", Environment.MachineName);
        _extent.AddSystemInfo("Test Run Time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        _extent.AddSystemInfo(".NET Version", Environment.Version.ToString());
        _extent.AddSystemInfo("Test Assembly", Assembly.GetExecutingAssembly().GetName().Name ?? "Unknown");
        
        // Add browser information if available
        var browser = Environment.GetEnvironmentVariable("TEST_BROWSER") ?? "chromium";
        var headless = Environment.GetEnvironmentVariable("TEST_HEADLESS") ?? "false";
        var timeout = Environment.GetEnvironmentVariable("TEST_TIMEOUT") ?? "30000";
        
        _extent.AddSystemInfo("Browser", browser);
        _extent.AddSystemInfo("Headless Mode", headless);
        _extent.AddSystemInfo("Default Timeout", $"{timeout}ms");
    }
    
    /// <summary>
    /// Create a test in the report
    /// </summary>
    public static ExtentTest CreateTest(string testName, string description = "")
    {
        if (_extent == null)
        {
            throw new InvalidOperationException("ExtentReports not initialized. Call InitializeReport() first.");
        }
        
        return _extent.CreateTest(testName, description);
    }
    
    /// <summary>
    /// Flush the report and generate the final HTML
    /// </summary>
    public static void FlushReport()
    {
        _extent?.Flush();
        
        if (File.Exists(_reportPath))
        {
            Console.WriteLine($"✅ Test report generated successfully: {_reportPath}");
            Console.WriteLine($"🌐 Open in browser: file:///{_reportPath.Replace("\\", "/")}");
        }
        else
        {
            Console.WriteLine("❌ Failed to generate test report");
        }
    }
    
    /// <summary>
    /// Get the current report path
    /// </summary>
    public static string GetReportPath() => _reportPath ?? "TestReport_Unknown.html";
    
    /// <summary>
    /// Delete all previous reports
    /// </summary>
    private static void DeleteAllReports()
    {
        try
        {
            if (!Directory.Exists(_reportDirectory)) return;
            
            var reportFiles = Directory.GetFiles(_reportDirectory, "TestReport_*.html");
            
            foreach (var file in reportFiles)
            {
                try
                {
                    File.Delete(file);
                    Console.WriteLine($"🗑️ Deleted previous report: {Path.GetFileName(file)}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Could not delete report {Path.GetFileName(file)}: {ex.Message}");
                }
            }
            
            if (reportFiles.Length > 0)
            {
                Console.WriteLine($"🧹 Cleaned up {reportFiles.Length} previous report(s)");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Error during report cleanup: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Add a screenshot to the test report
    /// </summary>
    public static void AddScreenshot(ExtentTest test, string screenshotPath, string description = "Screenshot")
    {
        if (File.Exists(screenshotPath))
        {
            test.AddScreenCaptureFromPath(screenshotPath, description);
        }
    }
    
    /// <summary>
    /// Log test step information
    /// </summary>
    public static void LogInfo(ExtentTest test, string message)
    {
        // Add timestamp to the log
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        test.Info($"[{timestamp}] {message}");
        Console.WriteLine($"ℹ️ {message}");
    }
    
    /// <summary>
    /// Log test step pass
    /// </summary>
    public static void LogPass(ExtentTest test, string message)
    {
        test.Pass(message);
        Console.WriteLine($"✅ {message}");
    }
    
    /// <summary>
    /// Log test step fail
    /// </summary>
    public static void LogFail(ExtentTest test, string message, Exception? exception = null)
    {
        if (exception != null)
        {
            test.Fail($"{message}: {exception.Message}");
            test.Fail(exception.StackTrace ?? "No stack trace available");
        }
        else
        {
            test.Fail(message);
        }
        Console.WriteLine($"❌ {message}");
    }
    
    /// <summary>
    /// Log test step warning
    /// </summary>
    public static void LogWarning(ExtentTest test, string message)
    {
        test.Warning(message);
        Console.WriteLine($"⚠️ {message}");
    }
    
    /// <summary>
    /// Log test step skip
    /// </summary>
    public static void LogSkip(ExtentTest test, string message)
    {
        test.Skip(message);
        Console.WriteLine($"⏭️ {message}");
    }
} 