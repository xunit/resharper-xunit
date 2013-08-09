using JetBrains.Application;
using JetBrains.Application.Settings.Storage.DefaultFileStorages;
using JetBrains.Application.Settings.UserInterface;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    [ShellComponent]
    public class SimpleExtensionManager
    {
        private readonly GlobalSettings globalSettings;

        public SimpleExtensionManager(GlobalSettings globalSettings)
        {
            this.globalSettings = globalSettings;
        }

        public bool IsInstalled()
        {
            return false;
        }

        public string ExtensionId
        {
            get { return "xunitcontrib"; }
        }

        public UserFriendlySettingsLayer.Identity SettingsMountPointId
        {
            get { return globalSettings.ProductGlobalLayerId; }
        }
   }
}