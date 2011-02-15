using System;
using System.Collections.Generic;

#if WINDOWS_PHONE

// Windows Phone doesn't have Assembly.CodeBase. I'm surprised SL3 does.

namespace Xunit
{
    namespace Sdk
    {
        internal class Assembly
        {
            private readonly System.Reflection.Assembly assembly;

            private Assembly(System.Reflection.Assembly assembly)
            {
                this.assembly = assembly;
            }

            public string CodeBase
            {
                get { return string.Empty; }
            }

            public IEnumerable<Type> GetExportedTypes()
            {
                return assembly.GetExportedTypes();
            }

            public static Assembly Load(System.Reflection.AssemblyName assemblyName)
            {
                return new Assembly(System.Reflection.Assembly.Load(assemblyName));
            }

            public Type GetType(string name)
            {
                return assembly.GetType(name);
            }
        }
    }
}

#endif