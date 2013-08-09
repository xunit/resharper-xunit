using JetBrains.Application;
using JetBrains.Application.Extensions;
using JetBrains.Application.Settings.UserInterface;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    [ShellComponent]
    public class SimpleExtensionManager
    {
        private readonly ExtensionManager extensionManager;
        private readonly ExtensionsSettingsMountPointProvider extensionsSettingsMountPointProvider;

        public SimpleExtensionManager(ExtensionManager extensionManager, ExtensionsSettingsMountPointProvider extensionsSettingsMountPointProvider)
        {
            this.extensionManager = extensionManager;
            this.extensionsSettingsMountPointProvider = extensionsSettingsMountPointProvider;
        }

        public bool IsInstalled()
        {
            // We might not be installed as an extension. This means dotCover,
            // debugging from the command line, or installed as an actual plugin
            // since the environment doesn't support the default nuget based
            // extension provider (e.g. VS2008)
            return extensionManager.IsInstalled(ExtensionId);
        }

        public string ExtensionId
        {
            get { return "xunitcontrib"; }
        }

        public UserFriendlySettingsLayer.Identity SettingsMountPointId
        {
            get { return extensionsSettingsMountPointProvider.Id; }
        }
    }
}