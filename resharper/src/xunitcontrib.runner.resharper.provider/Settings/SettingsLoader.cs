using System.Collections.Generic;
using JetBrains.Application;
using JetBrains.Application.FileSystemTracker;
using JetBrains.Application.Settings.Storage.DefaultFileStorages;
using JetBrains.Application.Settings.Storage.Persistence;
using JetBrains.Application.Settings.Store.Implementation;
using JetBrains.Application.Settings.UserInterface;
using JetBrains.DataFlow;
using JetBrains.Threading;
using JetBrains.Util;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider.Settings
{
    using LayerId = UserInjectedSettingsLayers.InjectedLayerPersistentIdentity;
    using LayerDescriptor = UserInjectedSettingsLayers.UserInjectedLayerDescriptor;
    using SaveEmptyFilePolicy = SettingsStoreSerializationToXmlDiskFile.SavingEmptyContent;
    using MountPath = SettingsStorageMountPoint.MountPath;

    // Settings and layers are a bit complex. Here's what's happening, and why:
    //
    // We're going to inject each dotSettings file as a layer, every time the plugin loads. Note that
    // if the plugin is disabled or uninstalled, no settings layer is created
    //
    // 1. Create a storage, based on a file
    //    We could create a readonly storage, from file or a stream, but using a file allows the user
    //    to write back to the file, as long as they explicitly select it in the appropriate UI.
    //    If we did create a readonly storage, the UI would allow us to "save" succesfully, but
    //    nothing would get persisted, surprising the user. Normal editing of the templates from this
    //    file will get saved to the Smart layer by default, and persisted to the This Computer
    //    dotSettings file (%APPDATA%\JetBrains\ReSharper\vAny\GlobalSettings.dotSettings)
    // 2. Create a layer description, using a constant as the persistent id
    //    The persistent id is the index in the key for the layer customisations (name, priority and
    //    enabled/disabled state). This will be persisted to the host layer (This Computer), so even
    //    though we add this layer on each startup, the customisations are persisted
    //    File injected layers work in a similar way, except they save file information with a guid
    //    id. On each startup, they add that file as a layer, with that guid as the persistent id, so
    //    customisations persist between startups
    // 3. Set the Delete function to do nothing
    //    We don't allow removing the layer from the UI. Either uninstall the plugin, or disable the
    //    layer - both have the same effect of removing the settings from the store. We override the
    //    DeleteInjectedLayerAction handler to check to hide the "Remove" item from the menu for
    //    our layer, so we shouldn't be called. If we are, we do nothing
    // 4. Set some metadata on the description
    //    This is for the UI. The DisplayName and Origin is just nice text to display. The other two
    //    are private to this plugin, and checked when our overridden actions are udpated or executed
    //    (see ActionOverrideRegistrar and Prevent*Action classes). This allows us to prevent the user
    //    from removing the layer and resetting the file back to an empty file
    // 5. Register the layer
    //    This creates an ISettingsStorageMountPoint and adds it to the store and also adds the layer 
    //    customisation. Since we don't get control of creating the mount point, we don't get to say
    //    if it should be treated as readonly, or defaults. It would be convenient to be default
    //    values, so that reset worked a whole lot nicer, but mount points classed as default values
    //    are excluded from the UI, and so we'd lose the ability to turn them on and off. It might
    //    also be nice to set a read-only flag. We could use a readonly xml storage, but then you
    //    don't get an indication that the layer is readonly when saving templates. At least this
    //    way, if you explicitly save to the template, it works. If you just edit and change, it
    //    gets saved to This Computer
    //
    // This all gets substantially easier in ReSharper 8.0
    [ShellComponent]
    public class SettingsLoader
    {
        public SettingsLoader(Lifetime lifetime, GlobalSettings globalSettings,
                              UserInjectedSettingsLayers userInjectedSettingsLayers, IThreading threading,
                              IFileSystemTracker filetracker, FileSettingsStorageBehavior behavior)
        {
            var files = GetSettingsFiles();
            var hostId = globalSettings.ProductGlobalLayerId;

            foreach (var file in files)
                MountSettingsFile(lifetime, "xunitcontrib", file, hostId, threading, filetracker, behavior, userInjectedSettingsLayers);
        }

        private void MountSettingsFile(Lifetime lifetime, string pluginId, FileSystemPath file,
                                       UserFriendlySettingsLayer.Identity hostId,
                                       IThreading threading, IFileSystemTracker filetracker,
                                       FileSettingsStorageBehavior behavior,
                                       UserInjectedSettingsLayers userInjectedSettingsLayers)
        {
            var id = string.Format("{0}-{1}", pluginId, file.NameWithoutExtension);
            var persistentId = new LayerId(id);

            var path = new Property<FileSystemPath>(lifetime, "InjectedFileStoragePath", file);
            var serialization = new XmlFileSettingsStorage(lifetime, id, path, SaveEmptyFilePolicy.KeepFile, threading,
                                                           filetracker, behavior);
            var descriptor = new LayerDescriptor(lifetime, hostId, persistentId, serialization.Storage,
                                                 MountPath.Default, () => { });

            descriptor.InitialMetadata.Set(UserFriendlySettingsLayers.DisplayName,
                                           string.Format("{0} » {1}", pluginId, file.NameWithoutExtension));
            descriptor.InitialMetadata.Set(UserFriendlySettingsLayers.Origin,
                                           string.Format("Published by plugin: {0}", pluginId));
            descriptor.InitialMetadata.Set(BelongsToPlugin, true);

            userInjectedSettingsLayers.RegisterUserInjectedLayer(lifetime, descriptor);
        }

        private IEnumerable<FileSystemPath> GetSettingsFiles()
        {
            return new FileSystemPath(GetType().Assembly.Location).Directory.GetChildFiles("*.dotSettings");
        }

        public static readonly PropertyId<bool> BelongsToPlugin = new PropertyId<bool>("BelongsToXunitcontrib");
    }
}