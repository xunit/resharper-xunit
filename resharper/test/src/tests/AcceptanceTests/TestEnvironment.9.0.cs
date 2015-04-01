using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Application;
using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.ReSharper.TestFramework;
using JetBrains.TestFramework;
using JetBrains.TestFramework.Application.Zones;
using JetBrains.Threading;
using NUnit.Framework;
using XunitContrib.Runner.ReSharper.RemoteRunner;
using XunitContrib.Runner.ReSharper.UnitTestProvider;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests
{
    [ZoneDefinition]
    public interface IXunitTestZone : ITestsZone, IRequire<PsiFeatureTestZone>
    {
    }

    /// <summary>
    /// Test environment. Must be in the root namespace of the tests
    /// </summary>
    [SetUpFixture]
    public class TestEnvironmentAssembly2 : TestEnvironmentAssembly<IXunitTestZone>
    {
        /// <summary>
        /// Gets the assemblies to load into test environment.
        /// Should include all assemblies which contain components.
        /// </summary>
        private static IEnumerable<Assembly> GetAssembliesToLoad()
        {
            // Test assembly
            yield return Assembly.GetExecutingAssembly();

            yield return typeof(XunitTestProvider).Assembly;
            yield return typeof(XunitTaskRunner).Assembly;
        }

        public override void SetUp()
        {
            base.SetUp();
            ReentrancyGuard.Current.Execute(
                "LoadAssemblies",
                () => Shell.Instance.GetComponent<AssemblyManager>().LoadAssemblies(
                    GetType().Name, GetAssembliesToLoad()));
        }

        public override void TearDown()
        {
            ReentrancyGuard.Current.Execute(
                "UnloadAssemblies",
                () => Shell.Instance.GetComponent<AssemblyManager>().UnloadAssemblies(
                    GetType().Name, GetAssembliesToLoad()));
            base.TearDown();
        }
    }
}
