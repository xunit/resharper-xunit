using System;
using System.Reflection;
using Microsoft.Silverlight.Testing;

#if !WINDOWS_PHONE
using System.Collections.Generic;
using System.Reflection.Emit;
#endif

namespace XunitContrib.Runner.Silverlight.Toolkit
{
    // I don't like this. All the Reflection.Emit stuff is because the unit test framework uses
    // the namespace of TestClass.Type, which is our helper class, not the class under test. So
    // for the desktop Silverlight versions, we build a new type in a dynamic assembly that has
    // the same name (including namespace) as the class under test (this class simply derives
    // from TestClassCommandTypeAdapter). This fools the framework into displaying the correct
    // namespace - it's just in the wrong assembly. Windows Phone doesn't support Reflection.Emit
    // so, we just use the class, and get the wrong namespace. Blech.
    public static class TestClassCommandTypeAdapterBuilder
    {
#if WINDOWS_PHONE
        public static Type GenerateAdapterType(Type testClass)
        {
            return ClassUnderTestHasExclusiveAttribute(testClass)
                        ? typeof (ExclusiveTestClassCommandTypeAdapter<>).MakeGenericType(testClass)
                        : typeof (TestClassCommandTypeAdapter<>).MakeGenericType(testClass);
        }

        private static bool ClassUnderTestHasExclusiveAttribute(ICustomAttributeProvider testClass)
        {
            return testClass.GetCustomAttributes(typeof(ExclusiveAttribute), true).Length > 0;
        }

#else

        private static ModuleBuilder moduleBuilder;
        private static readonly IDictionary<Type, Type> adapterTypeMap = new Dictionary<Type, Type>();

        public static Type GenerateAdapterType(Type testClass)
        {
            Type testClassCommandAdapter = null;
            if (!adapterTypeMap.TryGetValue(testClass, out testClassCommandAdapter))
            {
                // Note that we use the full, namespace qualified name of the type. Anyone reporting the name of the
                // type of the class will get it correct, it'll just be in the wrong assembly. Another quality hack.
                var typeBuilder = ModuleBuilder.DefineType(testClass.FullName, TypeAttributes.Public, typeof(TestClassCommandTypeAdapter<>).MakeGenericType(testClass));
                AddExclusiveAttribute(testClass, typeBuilder);
                testClassCommandAdapter = typeBuilder.CreateType();
                adapterTypeMap.Add(testClass, testClassCommandAdapter);
            }

            return testClassCommandAdapter;
        }

        private static void AddExclusiveAttribute(Type testClass, TypeBuilder typeBuilder)
        {
            if (testClass.GetCustomAttributes(typeof (ExclusiveAttribute), true).Length <= 0) return;

            var constructorInfo = typeof(ExclusiveAttribute).GetConstructor(new Type[0]);

            var exclusiveAttributeBuilder = new CustomAttributeBuilder(constructorInfo, new object[0]);
            typeBuilder.SetCustomAttribute(exclusiveAttributeBuilder);
        }

        private static ModuleBuilder ModuleBuilder
        {
            get
            {
                if (moduleBuilder == null)
                {
                    var assemblyName = new AssemblyName("xunit.silverlight.dynamic");
                    var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName,
                                                                                        AssemblyBuilderAccess.Run);
                    moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);
                }

                return moduleBuilder;
            }
        }
#endif
    }
}