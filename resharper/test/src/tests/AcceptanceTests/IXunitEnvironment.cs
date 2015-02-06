using System.Collections.Generic;
using JetBrains.Application.platforms;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.TestFramework;
using JetBrains.Util;

// ReSharper 8.2 doesn't define this, used by 9.0
namespace JetBrains.Application.platforms
{
}

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests
{
    public interface IXunitEnvironment : ITestPlatformProvider, ITestLibraryReferencesProvider
    {
        string Id { get; }

        // For compatibility with 8.2
        PlatformID GetPlatformID();
        IEnumerable<string> GetReferences(PlatformID platformId, FileSystemPath path);
    }

    public abstract class XunitEnvironmentBase
    {
        public PlatformID GetPlatformId()
        {
            return GetPlatformID();
        }

        public IEnumerable<string> GetReferences(PlatformID platformId)
        {
            return GetReferences(platformId, FileSystemPath.Empty);
        }

        public abstract PlatformID GetPlatformID();
        public abstract IEnumerable<string> GetReferences(PlatformID platformId, FileSystemPath path);
    }

    public class Xunit1Environment : XunitEnvironmentBase, IXunitEnvironment
    {
        public string Id { get { return "xunit1"; } }

        public override PlatformID GetPlatformID()
        {
            return PlatformID.CreateFromName(FrameworkIdentifier.NetFramework, new System.Version(3, 5),
                ProfileIdentifier.Default);
        }

        // TODO: Are these supposed to be fully qualified paths?
        public override IEnumerable<string> GetReferences(PlatformID platformId, FileSystemPath path)
        {
            yield return System.Environment.ExpandEnvironmentVariables(EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit191\xunit.dll");
            yield return System.Environment.ExpandEnvironmentVariables(EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit191\xunit.extensions.dll");
        }

        // TODO: What does this mean?
        public bool Inherits { get { return false; } }

        public override string ToString()
        {
            return Id;
        }
    }

    public class Xunit2Environment : XunitEnvironmentBase, IXunitEnvironment
    {
        public string Id { get { return "xunit2"; } }

        public override PlatformID GetPlatformID()
        {
            return PlatformID.CreateFromName(FrameworkIdentifier.NetFramework, new System.Version(4, 0),
                ProfileIdentifier.Default);
        }

        // TODO: Are these supposed to be fully qualified paths?
        public override IEnumerable<string> GetReferences(PlatformID platformId, FileSystemPath path)
        {
            yield return "System.Runtime.dll";
            yield return "System.Reflection.dll";
            yield return System.Environment.ExpandEnvironmentVariables(EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit2\xunit.abstractions.dll");
            yield return System.Environment.ExpandEnvironmentVariables(EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit2\xunit.core.dll");
            yield return System.Environment.ExpandEnvironmentVariables(EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit2\xunit.execution.desktop.dll");
            yield return System.Environment.ExpandEnvironmentVariables(EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit2\xunit.assert.dll");
        }

        // TODO: What does this mean?
        public bool Inherits { get { return false; } }

        public override string ToString()
        {
            return Id;
        }
    }
}