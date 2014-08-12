using System;
using System.Collections.Generic;
using JetBrains.Application;
using JetBrains.Metadata.Utils;
using JetBrains.ReSharper.Psi.Impl.Reflection2.ExternalAnnotations;
using JetBrains.Util;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Annotations
{
    [ShellComponent]
    public class ExternalAnnotationsProvider : IExternalAnnotationsFileProvider
    {
        private readonly OneToSetMap<string, FileSystemPath> annotations;
        private FileSystemPath cachedBaseTestDataPath;

        public ExternalAnnotationsProvider()
        {
            var location = TestDataPath2;
            annotations = new OneToSetMap<string, FileSystemPath>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in location.GetChildFiles("*.xml"))
                annotations.Add(file.NameWithoutExtension, file);
            foreach (var directory in location.GetChildDirectories())
            {
                foreach (var file in directory.GetChildFiles("*.xml", PathSearchFlags.RecurseIntoSubdirectories))
                {
                    annotations.Add(file.NameWithoutExtension, file);
                    annotations.Add(file.Directory.Name, file);
                }
            }
        }

        public IEnumerable<FileSystemPath> GetAnnotationsFiles(AssemblyNameInfo assemblyName = null, FileSystemPath assemblyLocation = null)
        {
            if (assemblyName == null)
                return annotations.Values;
            return annotations[assemblyName.Name];
        }

        private FileSystemPath BaseTestDataPath
        {
            get
            {
                if (cachedBaseTestDataPath == null)
                    cachedBaseTestDataPath = TestUtil.GetTestDataPathBase(GetType().Assembly);
                return cachedBaseTestDataPath;
            }
        }

        private string RelativeTestDataPath
        {
            get { return @"..\..\ExternalAnnotations"; }
        }

        private FileSystemPath TestDataPath2
        {
            get { return BaseTestDataPath.Combine(RelativeTestDataPath); }
        }
    }
}