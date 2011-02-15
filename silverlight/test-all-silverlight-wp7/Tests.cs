using System.Windows;
using Microsoft.Silverlight.Testing;
using Microsoft.Silverlight.Testing.Harness;
using XunitContrib.Runner.Silverlight.Toolkit;

namespace test.all.silverlight
{
    public static class Tests
    {
        public static UIElement CreateTestPage()
        {
            UnitTestProvider.Register();

            var unitTestSettings = UnitTestSystem.CreateDefaultSettings();
            unitTestSettings.TestAssemblies.Clear();
            unitTestSettings.TestAssemblies.Add(typeof(xunit_silverlight.XunitFixesTests).Assembly);
            unitTestSettings.TestAssemblies.Add(typeof(xunit.extensions_silverlight.XunitFixesTests).Assembly);
            unitTestSettings.ShowTagExpressionEditor = true;
            unitTestSettings.SampleTags = new[] { "All", "XunitFixesTests", "SanityCheckTests" };

            foreach (var logProvider in unitTestSettings.LogProviders)
            {
                if (logProvider is DebugOutputProvider)
                {
                    ((DebugOutputProvider) logProvider).ShowEverything = true;
                }
            }

            return UnitTestSystem.CreateTestPage(unitTestSettings);
        }
    }
}