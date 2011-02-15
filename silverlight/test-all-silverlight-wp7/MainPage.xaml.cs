using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Silverlight.Testing;
using test.all.silverlight;

namespace test_all_silverlight_wp7
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

            var testPage = Tests.CreateTestPage();
            BackKeyPress += (x, xe) => xe.Cancel = ((IMobileTestPage) testPage).NavigateBack();

            (Application.Current.RootVisual as PhoneApplicationFrame).Content = testPage;
        }
    }
}