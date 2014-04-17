using System;
using System.Collections.Generic;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.TestFramework;
using PlatformID = JetBrains.ProjectModel.PlatformID;

namespace XunitContrib.Runner.ReSharper.Tests
{
    public interface IXunitEnvironment : ITestPlatformProvider, ITestLibraryReferencesProvider
    {
         string Id { get; }
    }

    public class Xunit1Environment : IXunitEnvironment
    {
        public string Id { get { return "xunit1"; } }

        public PlatformID GetPlatformId()
        {
            return PlatformID.CreateFromName(FrameworkIdentifier.NetFramework, new Version(3, 5),
                ProfileIdentifier.Default);
        }

        // TODO: Are these supposed to be fully qualified paths?
        public IEnumerable<string> GetReferences(PlatformID platformId)
        {
            yield return Environment.ExpandEnvironmentVariables(EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit191\xunit.dll");
            yield return Environment.ExpandEnvironmentVariables(EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit191\xunit.extensions.dll");
        }

        // TODO: What does this mean?
        public bool Inherits { get { return false; } }

        public override string ToString()
        {
            return Id;
        }
    }

    public class Xunit2Environment : IXunitEnvironment
    {
        public string Id { get { return "xunit2"; } }

        public PlatformID GetPlatformId()
        {
            return PlatformID.CreateFromName(FrameworkIdentifier.NetFramework, new Version(4, 0),
                ProfileIdentifier.Default);
        }

        // TODO: Are these supposed to be fully qualified paths?
        public IEnumerable<string> GetReferences(PlatformID platformId)
        {
            yield return "System.Runtime.dll";
            yield return Environment.ExpandEnvironmentVariables(EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit2\xunit.abstractions.dll");
            yield return Environment.ExpandEnvironmentVariables(EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit2\xunit.core.dll");
            yield return Environment.ExpandEnvironmentVariables(EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit2\xunit.execution.dll");
            yield return Environment.ExpandEnvironmentVariables(EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit2\xunit.assert.dll");
        }

        // TODO: What does this mean?
        public bool Inherits { get { return false; } }

        public override string ToString()
        {
            return Id;
        }
    }
}