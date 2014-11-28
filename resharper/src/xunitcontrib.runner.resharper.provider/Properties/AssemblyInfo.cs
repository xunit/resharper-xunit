using System.Reflection;
using JetBrains.Application.PluginSupport;

[assembly: AssemblyTitle("xUnit.net design time unit test provider for " + ProductInfo.Product + " " + ProductInfo.Version)]

[assembly: PluginTitle("xUnit.net unit test provider")]
[assembly: PluginDescription("Allows " + ProductInfo.Product + " to run xUnit.net tests. Compiled against " + ProductInfo.Product + " " + ProductInfo.Version)]
[assembly: PluginVendor("Matt Ellis")]

