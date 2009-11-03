using System.Reflection;
using System.Runtime.InteropServices;
using JetBrains.UI.Application.PluginSupport;

[assembly : AssemblyTitle("xUnit.net unit test provider for ReSharper 4.5")]
[assembly : PluginTitle("xUnit.net unit test provider for ReSharper 4.5")]
[assembly : PluginDescription("Allows ReSharper 4.5 to run unit tests from xUnit.net")]
[assembly : PluginVendor("xunitcontrib Team")]

// Temporarily copied from CommonAssemblyInfo so that I can maintain a separate build number
[assembly: AssemblyCompany("http://xunitcontrib.codeplex.com")]
[assembly: AssemblyProduct("xunitcontrib.runner.resharper")]
[assembly: AssemblyDescription("xUnit.net unit test provider for Resharper")]
[assembly: AssemblyCopyright("Copyright (C) xunitcontrib team")]
[assembly: ComVisible(false)]

// Note that the build number matches the build of ReSharper 5.0 built against
[assembly: AssemblyVersion("0.3.2.1527")]