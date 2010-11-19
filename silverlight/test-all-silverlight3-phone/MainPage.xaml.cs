using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Silverlight.Testing;
using Microsoft.Silverlight.Testing.Harness;
using XunitContrib.Runner.Silverlight.Toolkit;

namespace test_all_silverlight3_phone
{
    public partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();

            Loaded += MainPage_Loaded;
        }

        public void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            SystemTray.IsVisible = false;

            UnitTestProvider.Register();

            var settings = new UnitTestSettings {StartRunImmediately = true, TestHarness = new UnitTestHarness()};
            settings.TestAssemblies.Add(typeof(test.xunit_silverlight.App).Assembly);
            settings.TestAssemblies.Add(typeof(test.xunit.extensions_silverlight.App).Assembly);

            var testPage = UnitTestSystem.CreateTestPage(settings) as IMobileTestPage;
            BackKeyPress += (x, xe) => xe.Cancel = testPage.NavigateBack();
            (Application.Current.RootVisual as PhoneApplicationFrame).Content = testPage;
        }
    }
}