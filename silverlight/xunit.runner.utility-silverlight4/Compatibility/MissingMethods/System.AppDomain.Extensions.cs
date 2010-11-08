using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Xunit
{
    internal static partial class Extensions
    {
        // Silverlight doesn't implement this.
        internal static AssemblyBuilder DefineDynamicAssembly(this AppDomain appDomain, AssemblyName assemblyName, AssemblyBuilderAccess access, CustomAttributeBuilder[] attributeBuilders)
        {
            return appDomain.DefineDynamicAssembly(assemblyName, access);
        }
    }
}