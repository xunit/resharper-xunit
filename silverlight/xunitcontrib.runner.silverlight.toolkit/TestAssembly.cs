using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Silverlight.Testing.Harness;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using Xunit.Sdk;

namespace XunitContrib.Runner.Silverlight.Toolkit
{
    public class TestAssembly : IAssembly
    {
        private readonly UnitTestHarness testHarness;
        private readonly Assembly assembly;
        private readonly IUnitTestProvider provider;

        public TestAssembly(UnitTestHarness testHarness, Assembly assembly, IUnitTestProvider provider)
        {
            this.testHarness = testHarness;
            this.assembly = assembly;
            this.provider = provider;
        }

        public MethodInfo AssemblyInitializeMethod
        {
            get { return null; }
        }

        public MethodInfo AssemblyCleanupMethod
        {
            get { return null; }
        }

        public ICollection<ITestClass> GetTestClasses()
        {
            var testClasses = from type in assembly.GetExportedTypes()
                              where TypeUtility.IsTestClass(Reflector.Wrap(type))
                              select new TestClass(type, this) as ITestClass;

            return testClasses.ToList();
        }

        public IUnitTestProvider Provider
        {
            get { return provider; }
        }

        public string Name
        {
            get
            {
                // Oddly, Assembly.GetName() is security critical...
                var fullName = assembly.FullName;
                return (fullName.Contains(", ") ? fullName.Substring(0, fullName.IndexOf(",", StringComparison.Ordinal)) : fullName);
            }
        }

        public UnitTestHarness TestHarness
        {
            get { return testHarness; }
        }
    }
}