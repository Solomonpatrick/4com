using NUnit.Framework;
using AutomationExercise.Tests.Helpers;

namespace AutomationExercise.Tests.Tests;

/// <summary>
/// Global test setup and teardown for the entire test assembly
/// </summary>
[SetUpFixture]
public class GlobalTestSetup
{
    /// <summary>
    /// Runs once before any tests in the assembly
    /// </summary>
    [OneTimeSetUp]
    public void AssemblySetUp()
    {
        // Install Playwright if needed
        Microsoft.Playwright.Program.Main(new[] { "install" });
        
        // Initialize the reporting system once for all tests
        ReportManager.InitializeReport();
        
        TestContext.WriteLine("🚀 Global test setup completed - Report initialized");
    }
    
    /// <summary>
    /// Runs once after all tests in the assembly
    /// </summary>
    [OneTimeTearDown]
    public void AssemblyTearDown()
    {
        // Generate the final report after all tests complete
        ReportManager.FlushReport();
        
        TestContext.WriteLine("✅ Global test teardown completed - Report generated");
    }
} 