using System;
using JetBrains.Application.platforms;
using JetBrains.Util;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests
{
    public static class CompileCs
    {
        public static void Compile(IFrameworkDetectionHelper frameworkDetectionHelper, FileSystemPath source, FileSystemPath dll,
            string[] references, Version version)
        {
            // The 9.1 tests are using the test nuget packages to allow testing any framework,
            // but there's a bug in the JetBrains.Tests.Platform.NetFramework.Binaries.4.0 package
            // that means csc.exe doesn't work. Use the system 4.x compiler.
            // https://youtrack.jetbrains.com/issue/RSRP-437176
            CompileUtil.CompileCs(new SystemFrameworkLocationHelper(), source, dll, references, false,
                true, version);
        }
    }
}