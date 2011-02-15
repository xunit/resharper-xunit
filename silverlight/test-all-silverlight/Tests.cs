using System.Windows;
using Microsoft.Silverlight.Testing;
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
            unitTestSettings.TestAssemblies.Add(typeof(xunit_silverlight.App).Assembly);
            unitTestSettings.TestAssemblies.Add(typeof(xunit.extensions_silverlight.App).Assembly);
            unitTestSettings.ShowTagExpressionEditor = true;
            unitTestSettings.SampleTags = new[] { "All", "XunitFixesTests", "SanityCheckTests" };

            return UnitTestSystem.CreateTestPage(unitTestSettings);
        }
    }
}