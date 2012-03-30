using System.Configuration;
using NUnit.Framework;

namespace tests.reference.nunit
{
    namespace ReadsFromConfigFile
    {
        [TestFixture]
        public class ReadsFromConfigFile
        {
            [Test]
            public void ReadsAppSettings()
            {
                Assert.AreEqual("ValueFromAppSettings", ConfigurationManager.AppSettings["SettingsKey"]);
            }
        }
    }
}