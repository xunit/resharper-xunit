using System.Reflection;
using System.Runtime.CompilerServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("test_all_silverlight_wp7")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("xunitcontrib")]
[assembly: AssemblyProduct("test_all_silverlight_wp7")]
[assembly: AssemblyCopyright("Copyright © xunitcontrib 2010")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Silverlight doesn't allow private reflection. You can only get access to members that
// you would be able to access staticly. Allow xunit to access our internals
[assembly: InternalsVisibleTo("xunit-silverlight-wp7")]
