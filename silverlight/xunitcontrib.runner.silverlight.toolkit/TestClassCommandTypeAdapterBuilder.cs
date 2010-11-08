using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Silverlight.Testing;

namespace XunitContrib.Runner.Silverlight.Toolkit
{
    public static class TestClassCommandTypeAdapterBuilder
    {
        private static ModuleBuilder moduleBuilder;
        private static readonly IDictionary<Type, Type> adapterTypeMap = new Dictionary<Type, Type>();

        public static Type GetAdapterType(Type testClass)
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
    }
}