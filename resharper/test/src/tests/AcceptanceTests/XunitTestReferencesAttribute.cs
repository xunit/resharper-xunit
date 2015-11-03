using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using JetBrains.Application.platforms;
using JetBrains.ReSharper.TestFramework;
using NuGet;
using PlatformID = JetBrains.Application.platforms.PlatformID;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests
{
    public interface IXunitEnvironment : ITestPlatformProvider, ITestLibraryReferencesProvider
    {
        string Id { get; }
    }

    public class Xunit1TestReferencesAttribute : TestPackagesAttribute, IXunitEnvironment
    {
        private readonly bool supportAsync;

        public Xunit1TestReferencesAttribute(bool supportAsync = false)
        {
            this.supportAsync = supportAsync;
            Packages = new[]
            {
                "xunit/1.9.2",
                "xunit.extensions/1.9.2"
            };
            Inherits = true;
        }

        // TODO: Can we get rid of this? I think I'd prefer to set it on the tests
        // Unfortunately, it's used by the output tests which aren't using this as an attribute
        public PlatformID GetPlatformID()
        {
            var version = supportAsync ? new Version(4, 0) : new Version(3, 5);
            return PlatformID.CreateFromName(FrameworkIdentifier.NetFramework,
                version, ProfileIdentifier.Default);
        }

        public string Id { get { return "xunit1"; } }

        public override string ToString()
        {
            return Id;
        }
    }

    public class Xunit2TestReferencesAttribute : TestPackagesAttribute, IXunitEnvironment
    {
        public Xunit2TestReferencesAttribute()
            : base("System.Runtime.dll", "System.Reflection.dll")
        {
            Packages = new[] { "xunit/2.1.0" };
            Inherits = true;
        }

        protected override IEnumerable<string> GetPackageReferences(IPackageManager packageManager, IPackage package, FrameworkName framework)
        {
            if (package.Id == "xunit.extensibility.core")
            {
                // Filter out assemblies that aren't referenced (xunit.runner.tdnet.dll and xunit.runner.utility.desktop.dll)
                return base.GetPackageReferences(packageManager, package, framework)
                    .Where(f => f.EndsWith("xunit.core.dll"));
            }

            return base.GetPackageReferences(packageManager, package, framework);
        }

        public PlatformID GetPlatformID()
        {
            return PlatformID.CreateFromName(FrameworkIdentifier.NetFramework, new System.Version(4, 5),
                ProfileIdentifier.Default);
        }

        public string Id { get { return "xunit2"; } }

        public override string ToString()
        {
            return Id;
        }
    }
}