using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application;
using JetBrains.Application.Env;
using JetBrains.Application.Extensions;
using JetBrains.Application.PluginSupport;
using JetBrains.DataFlow;
using JetBrains.Util;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider.Annotations
{
  // NOTE: You copy/paste this class and simply change ExtensionId
  [EnvironmentComponent(Sharing.Common)]
  public class ExtensionInstaller
  {
    private const string ExtensionId = "xunitcontrib";

    public ExtensionInstaller(ExtensionManager extensionManager, ExtensionsFromPluginProvider myExtensionProvider)
    {
      if (!extensionManager.IsInstalled(ExtensionId))
        myExtensionProvider.Initialise(ExtensionId);
    }
  }

  // NOTE: You copy/paste this class and don't change it. It does all the work
  [EnvironmentComponent(Sharing.Product)]
  public class ExtensionsFromPluginProvider : IExtensionProvider
  {
    private readonly ExtensionLocations extensionLocations;
    private readonly CollectionEvents<IExtension> extensions;
    private readonly Lifetime lifetime;
    private readonly PluginsDirectory pluginsDirectory;

    public ExtensionsFromPluginProvider(Lifetime lifetime, ExtensionLocations extensionLocations,
      PluginsDirectory pluginsDirectory)
    {
      this.lifetime = lifetime;
      this.extensionLocations = extensionLocations;
      this.pluginsDirectory = pluginsDirectory;
      extensions = new CollectionEvents<IExtension>(lifetime, "ExtensionsFromPluginProvider");
    }

    public string Name { get { return "plugins"; } }
    public IViewable<IExtension> Extensions { get { return extensions; } }

    public void Initialise(string extensionId)
    {
      Plugin plugin = pluginsDirectory.Plugins.FirstOrDefault(p => p.ID == extensionId);
      if (plugin == null)
      {
        plugin = (from p in pluginsDirectory.Plugins
          from a in p.AssemblyPaths
          where a == FileSystemPath.CreateByCanonicalPath(GetType().Assembly.Location)
          select p).FirstOrDefault();
      }

      if (plugin != null)
        extensions.Add(lifetime, new ExtensionFromPlugin(lifetime, plugin, this, extensionLocations));
    }

    private class ExtensionFromPlugin : IExtension
    {
      private readonly ExtensionLocations extensionLocations;
      private readonly Plugin plugin;

      public ExtensionFromPlugin(Lifetime lifetime, Plugin plugin, IExtensionProvider provider,
        ExtensionLocations extensionLocations)
      {
        this.plugin = plugin;
        this.extensionLocations = extensionLocations;
        Id = plugin.ID;
        Enabled = new Property<bool?>(lifetime, string.Format("ExtensionFromPlugin:{0}", Id));
        RuntimeInfoRecords = new ListEvents<ExtensionRecord>(lifetime,
          string.Format("ExtensionFromPlugin:{0}", Id));
        Source = provider;
        Metadata = new ExtensionFromPluginMetadata(plugin);
      }

      public IEnumerable<FileSystemPath> GetFiles(string fileType)
      {
        // Don't return any plugins - we'd be loading ourself!
        if (fileType == "plugins")
          yield break;

        List<FileSystemPath> searchLocations =
          extensionLocations.ExtensionComponentSearchPaths.Concat(FileSystemPath.Empty).ToList();
        List<FileSystemPath> pluginLocations = plugin.AssemblyPaths.Select(p => p.Directory).Distinct().ToList();
        FileSystemPath root = pluginLocations.Count > 1
          ? pluginLocations.Aggregate(FileSystemPath.GetDeepestCommonParent)
          : pluginLocations[0];
        List<FileSystemPath> files = (from pluginLocation in pluginLocations
          from searchLocation in searchLocations
          let directory = pluginLocation.Combine(searchLocation.Combine(fileType))
          from file in directory.GetChildFiles("*", PathSearchFlags.RecurseIntoSubdirectories)
          select file).ToList();

        ReportFiles(fileType, root, files);

        foreach (FileSystemPath file in files)
          yield return file;
      }

      public string Id { get; private set; }
      public Version Version { get { return plugin.Presentation.AssemblyNameInfo.Version; } }
      public string SemanticVersion { get { return Version.ToString(); } }
      public IExtensionMetadata Metadata { get; private set; }
      public bool Supported { get { return true; } }
      public IProperty<bool?> Enabled { get; private set; }
      public ListEvents<ExtensionRecord> RuntimeInfoRecords { get; private set; }
      public IExtensionProvider Source { get; private set; }

      private void ReportFiles(string fileType, FileSystemPath root, IList<FileSystemPath> files)
      {
        if (files.Any())
        {
          this.AddInfo(this, string.Format("Found {0} files under {1}", files.Count, root));
        }
        else
        {
          this.AddInfo(this, string.Format("The package contains no files in {0}", root));
        }
      }
    }

    private class ExtensionFromPluginMetadata : IExtensionMetadata
    {
      private readonly Plugin plugin;

      public ExtensionFromPluginMetadata(Plugin plugin)
      {
        this.plugin = plugin;
        Copyright = null; // TODO: Load a plugin assembly looking for AssemblyCopyrightAttribute
      }

      public string Title { get { return plugin.Presentation.Title; } }
      public string Description { get { return plugin.Presentation.Description; } }
      public string Summary { get { return string.Empty; } }
      public string Copyright { get; private set; }
      public IEnumerable<string> Authors { get { return new[] {plugin.Presentation.Vendor}; } }
      public IEnumerable<string> Owners { get { return Authors; } }
      public IEnumerable<string> Tags { get { return new string[0]; } }
      public IEnumerable<string> DependencyIds { get { return new string[0]; } }
      public IEnumerable<string> DependencyDescriptions { get { return new string[0]; } }
      public Uri IconUrl { get { return null; } }
      public Uri LicenseUrl { get { return null; } }
      public Uri ProjectUrl { get { return null; } }
      public DateTimeOffset? Created { get { return null; } }
      public bool PreRelease { get { return false; } }
    }
  }
}