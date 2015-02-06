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
            CompileUtil.CompileCs(frameworkDetectionHelper, source, dll, references, false,
                false, version);
        }
    }
}