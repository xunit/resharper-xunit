using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Application;
using JetBrains.Threading;
using NUnit.Framework;
using XunitContrib.Runner.ReSharper.RemoteRunner;
using XunitContrib.Runner.ReSharper.UnitTestProvider;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests
{
    /// <summary>
    /// Test environment. Must be in the root namespace of the tests
    /// </summary>
    [SetUpFixture]
    public class TestEnvironmentAssembly : ReSharperTestEnvironmentAssembly
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
            var sw = Stopwatch.StartNew();

            base.SetUp();
            ReentrancyGuard.Current.Execute(
                "LoadAssemblies",
                () => Shell.Instance.GetComponent<AssemblyManager>().LoadAssemblies(
                    GetType().Name, GetAssembliesToLoad()));

            Console.WriteLine("Startup took: {0}", sw.Elapsed);
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
