using System.Collections.Generic;
using System.IO;
using System.Threading;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Utils;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.TaskRunnerFramework;
using JetBrains.ReSharper.TestFramework.Components.UnitTestSupport;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestSupportTests;
using JetBrains.Util;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests
{
    public abstract partial class UnitTestTaskRunnerTestBase
    {
        private int GetServerPortNumber()
        {
            var unitTestServer = Solution.GetComponent<UnitTestServer>();
            return unitTestServer.PortNumber;
        }

        private IRemoteChannel GetRemoteChannel()
        {
            return null;
        }

        protected abstract IUnitTestMetadataExplorer MetadataExplorer { get; }

        protected virtual ICollection<IUnitTestElement> GetUnitTestElements(IProject testProject, string assemblyLocation)
        {
            var metadataExplorer = MetadataExplorer;
            var tests = new List<IUnitTestElement>();

            string assemblyName = testProject.GetSubItems()[0].Location.Name;
            var testAssembly = FileSystemPath.Parse(GetTestDataFilePath(assemblyName));

            var resolver = new DefaultAssemblyResolver();
            resolver.AddPath(testAssembly.Directory);

            using (var loader = new MetadataLoader(resolver))
            {
                var metadataAssembly = loader.LoadFrom(testAssembly, JetFunc<AssemblyNameInfo>.True);
                using (var assemblyLoader = new AssemblyLoader())
                {
                    assemblyLoader.RegisterPath(BaseTestDataPath.Combine(RelativeTestDataPath).FullPath);
                    metadataExplorer.ExploreAssembly(testProject, metadataAssembly, tests.Add,
                        new ManualResetEvent(false));
                }
            }

            return tests;
        }

        protected virtual ITaskRunnerHostController CreateTaskRunnerHostController(IUnitTestLaunchManager launchManager, IUnitTestResultManager resultManager, IUnitTestAgentManager agentManager, TextWriter output, IUnitTestLaunch launch, int port, TestRemoteChannelMessageListener msgListener)
        {
            return new ProcessTaskRunnerHostController(launchManager, agentManager, launch, port);
        }

        // Hook for a 9.1 workaround
        private static IUnitTestLaunch GetUnitTestLaunch(UnitTestSessionTestImpl session)
        {
            return session;
        }
    }
}