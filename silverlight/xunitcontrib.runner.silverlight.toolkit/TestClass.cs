using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;

namespace XunitContrib.Runner.Silverlight.Toolkit
{
    public class TestClass : ITestClass
    {
        private readonly Type type;
        private readonly IAssembly assembly;
        private Type helperType;

        private static ModuleBuilder moduleBuilder;

        public TestClass(Type type, IAssembly assembly)
        {
            this.assembly = assembly;
            this.type = type;
        }

        // Returning back a different type allows us to wedge ourselves between the actual test class
        // and the metadata returned to the test runner (we can use TestClassCommand's ClassStart and
        // ClassFinish methods to handle IUseFixture, and we can implement IProvideDynamicTestMethods)
        // The only downside is that we lose namespaces - all tests classes start showing the helper
        // type's namespace. So we generate a new type dynamically, for each test class
        public Type Type
        {
            get { return helperType ?? (helperType = TestClassCommandTypeAdapterBuilder.GetAdapterType(type)); }
        }

        public ICollection<ITestMethod> GetTestMethods()
        {
            // Don't return any tests here - we'll do everything in IProvideDynamicTestMethods
            return Enumerable.Empty<ITestMethod>().ToList();
        }

        public bool Ignore
        {
            // xunit doesn't support ignoring whole classes
            get { return false; }
        }

        public MethodInfo TestInitializeMethod
        {
            // xunit's per test cleanup is the constructor, handled by the ITestCommand, which creates
            // a new class instance per method run
            get { return null; }
        }

        public MethodInfo TestCleanupMethod
        {
            // xunit's per test cleanup is the dispose method, handled by the ITestCommand when each
            // test method is run
            get { return null; }
        }

        public MethodInfo ClassInitializeMethod
        {
            // Call the ITestClassCommand.ClassStart method to initialise any usages of IUseFixture<>
            get { return helperType.GetMethod("ClassStart", BindingFlags.Public | BindingFlags.Instance); }
        }

        public MethodInfo ClassCleanupMethod
        {
            // Call the ITestClassCommand.ClassFinish method to clean up any usages of IUseFixture<>
            get { return helperType.GetMethod("ClassFinish", BindingFlags.Public | BindingFlags.Instance); }
        }

        public string Name
        {
            get { return type.Name; }
        }

        public IAssembly Assembly
        {
            get { return assembly; }
        }
    }
}