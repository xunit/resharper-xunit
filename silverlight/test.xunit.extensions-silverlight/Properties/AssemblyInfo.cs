using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("xunit.extensions-silverlight (ported by xunitcontrib)")]
[assembly: AssemblyDescription("Silverlight port of xunit.extensions.dll (from the xunitcontrib project)")]

// Silverlight reflection doesn't allow us to access anything we shouldn't already be
// able to access staticly. Some of the tests in this assembly use reflection against
// non-public methods
[assembly: InternalsVisibleTo("xunit-silverlight4")]
[assembly: InternalsVisibleTo("xunit-silverlight3")]
[assembly: InternalsVisibleTo("xunit-silverlight-wp7")]