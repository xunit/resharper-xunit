using System;
using JetBrains.Util;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests
{
    // ReSharper disable InconsistentNaming
    public static class EnvironmentVariables
    {
        public const string XUNIT_ASSEMBLIES_ENV_VAR = "XUNIT_ASSEMBLIES";

        public const string XUNIT_ASSEMBLIES = "%" + XUNIT_ASSEMBLIES_ENV_VAR + "%";

        public static void SetUp(FileSystemPath baseDataPath)
        {
            var assembliesPath = baseDataPath.Directory.Combine("lib");
            Environment.SetEnvironmentVariable(XUNIT_ASSEMBLIES_ENV_VAR,
                assembliesPath.FullPath + @"\", EnvironmentVariableTarget.Process);
        }

        public static string XunitAssembliesPath
        {
            get { return Environment.ExpandEnvironmentVariables(XUNIT_ASSEMBLIES); }
        }
    }
    // ReSharper restore InconsistentNaming
}