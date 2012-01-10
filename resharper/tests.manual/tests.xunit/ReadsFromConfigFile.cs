using System.Configuration;
using Xunit;

namespace tests.xunit
{
    namespace ReadsFromConfigFile
    {
        public class ReadsFromConfigFile
        {
            // TEST: Should read value from app.config/appSettings
            [Fact]
            public void ReadsAppSettings()
            {
                Assert.Equal("ValueFromAppSettings", ConfigurationManager.AppSettings["SettingsKey"]);
            }
        }
    }
}