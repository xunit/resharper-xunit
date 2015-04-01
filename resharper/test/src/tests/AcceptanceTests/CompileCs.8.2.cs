using System;
using JetBrains.ProjectModel.impl;
using JetBrains.Util;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests
{
    public static class CompileCs
    {
        public static void Compile(IFrameworkDetectionHelper frameworkDetectionHelper, FileSystemPath source, FileSystemPath dll,
            string[] references, Version version)
        {
            CompileUtil.CompileCs(source, dll, references, false, false, version.ToString(2));
        }
    }
}