using JetBrains.Application;
using JetBrains.Application.FileSystemTracker;
using JetBrains.Application.Settings.Storage;
using JetBrains.Application.Settings.Storage.Persistence;
using JetBrains.Application.Settings.UserInterface;
using JetBrains.DataFlow;
using JetBrains.Threading;
using JetBrains.Util;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider.Settings
{
    using SavingEmptyContent = SettingsStoreSerializationToXmlDiskFile.SavingEmptyContent;

    public partial class SettingsLoader
    {
        public static readonly PropertyId<bool> IsNonUserEditable = UserFriendlySettingsLayers.IsNonUserEditable;

        private static XmlFileSettingsStorage CreateXmlFileSettingsStorage(Lifetime lifetime, IThreading threading, IFileSystemTracker filetracker, FileSettingsStorageBehavior behavior, string id, Property<FileSystemPath> pathAsProperty)
        {
            var internKeyPathComponent = Shell.Instance.GetComponent<InternKeyPathComponent>();
            return new XmlFileSettingsStorage(lifetime, id, pathAsProperty, SavingEmptyContent.KeepFile,
                threading, filetracker, behavior, internKeyPathComponent);
        }
    }
}