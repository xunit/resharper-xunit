using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Application;
using JetBrains.Metadata.Utils;
using JetBrains.ReSharper.Psi.Impl.Reflection2.ExternalAnnotations;
using JetBrains.Util;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider.Annotations
{
    [ShellComponent]
    public class AnnotationsLoader : IExternalAnnotationsFileProvider
    {
        private readonly OneToSetMap<string, FileSystemPath> myAnnotations;
        private readonly SimpleExtensionManager extensionManager;

        public AnnotationsLoader(SimpleExtensionManager extensionManager)
        {
            this.extensionManager = extensionManager;

            var location = FileSystemPath.CreateByCanonicalPath(Assembly.GetExecutingAssembly().Location)
                .Directory.Combine("ExternalAnnotations");
            myAnnotations = new OneToSetMap<string, FileSystemPath>(StringComparer.OrdinalIgnoreCase);

            // Cache the annotation filenames to save scanning the directory multiple times.
            // Safe to cache as the user shouldn't be changing files in the install dir. If
            // they want to add extra annotations, use an extension.
            // The rules are simple: either the file is named after the assembly, or the folder
            // is (or both, but that doesn't matter)
            foreach (var file in location.GetChildFiles("*.xml"))
                myAnnotations.Add(file.NameWithoutExtension, file);
            foreach (var directory in location.GetChildDirectories())
            {
                foreach (var file in directory.GetChildFiles("*.xml", PathSearchFlags.RecurseIntoSubdirectories))
                {
                    myAnnotations.Add(file.NameWithoutExtension, file);
                    myAnnotations.Add(file.Directory.Name, file);
                }
            }
        }

        public IEnumerable<FileSystemPath> GetAnnotationsFiles(AssemblyNameInfo assemblyName,
            FileSystemPath assemblyLocation)
        {
            return extensionManager.IsInstalled() ? EmptyList<FileSystemPath>.InstanceList : myAnnotations[assemblyName.Name];
        }
    }
}