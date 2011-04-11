using System.Reflection;
using System.Runtime.InteropServices;
using JetBrains.UI.Application.PluginSupport;

[assembly : AssemblyTitle("xUnit.net unit test provider for ReSharper 5.1")]
[assembly : PluginTitle("xUnit.net unit test provider for ReSharper 5.1")]
[assembly : PluginDescription("Allows ReSharper 5.1 to run unit tests from xUnit.net")]
[assembly : PluginVendor("xunitcontrib Team")]

[assembly: AssemblyCompany("http://xunitcontrib.codeplex.com")]
[assembly: AssemblyProduct("xunitcontrib.runner.resharper")]
[assembly: AssemblyDescription("xUnit.net unit test provider for Resharper")]
[assembly: AssemblyCopyright("Copyright (C) xunitcontrib team")]
[assembly: ComVisible(false)]

// This build number will track the ReSharper 5.1 nightly build number
[assembly: AssemblyVersion("0.4.1.1709")]