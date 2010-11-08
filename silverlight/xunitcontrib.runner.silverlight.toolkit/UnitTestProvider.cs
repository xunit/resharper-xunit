using System;
using System.Reflection;
using Microsoft.Silverlight.Testing;
using Microsoft.Silverlight.Testing.Harness;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;

namespace XunitContrib.Runner.Silverlight.Toolkit
{
    public class UnitTestProvider : IUnitTestProvider
    {
        public static void Register()
        {
            UnitTestSystem.RegisterUnitTestProvider(new UnitTestProvider());
        }

        public IAssembly GetUnitTestAssembly(UnitTestHarness testHarness, Assembly assemblyReference)
        {
            return new TestAssembly(testHarness, assemblyReference, this);
        }

        public string Name
        {
            get { return "xUnit.NET"; }
        }

        public UnitTestProviderCapabilities Capabilities
        {
            get { return UnitTestProviderCapabilities.MethodCanIgnore; }
        }

        public bool HasCapability(UnitTestProviderCapabilities capability)
        {
            return (Capabilities & capability) == capability;
        }

        public bool IsFailedAssert(Exception exception)
        {
            return true;
        }
    }
}