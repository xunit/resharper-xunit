using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("tests.xunit.extensions-silverlight (ported by xunitcontrib)")]
[assembly: AssemblyDescription("Silverlight port of tests.xunit.extensions.dll (from the xunitcontrib project)")]

// Silverlight doesn't allow private reflection. You can only get access to members that
// you would be able to access staticly. Allow xunit to access our internals
[assembly: InternalsVisibleTo("xunit-silverlight4")]
[assembly: InternalsVisibleTo("xunit-silverlight3")]
[assembly: InternalsVisibleTo("xunit-silverlight-wp7")]