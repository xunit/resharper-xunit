using JetBrains.Application.FileSystemTracker;
using JetBrains.Application.Settings.Storage.Persistence;
using JetBrains.DataFlow;
using JetBrains.Threading;
using JetBrains.Util;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider.Settings
{
    using SaveEmptyFilePolicy = SettingsStoreSerializationToXmlDiskFile.SavingEmptyContent;

    public partial class SettingsLoader
    {
        public static readonly PropertyId<bool> IsNonUserEditable = new PropertyId<bool>("BelongsToXunitcontrib");
        private static XmlFileSettingsStorage CreateXmlFileSettingsStorage(Lifetime lifetime, IThreading threading, IFileSystemTracker filetracker, FileSettingsStorageBehavior behavior, string id, Property<FileSystemPath> pathAsProperty)
        {
            return new XmlFileSettingsStorage(lifetime, id, pathAsProperty, SaveEmptyFilePolicy.KeepFile,
                threading, filetracker, behavior);
        }
    }
}