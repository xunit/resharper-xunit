using System.Collections.Generic;
using System.IO;
using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.TaskRunnerFramework;
using JetBrains.ReSharper.TestFramework.Components.UnitTestSupport;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.Util;
using JetBrains.Util.Logging;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests
{
    public abstract partial class UnitTestTaskRunnerTestBase
    {
        private int GetServerPortNumber()
        {
            var unitTestServer = Solution.GetComponent<UnitTestServerTestImpl>();
            return unitTestServer.PortNumber;
        }

        private IRemoteChannel GetRemoteChannel()
        {
            var unitTestServer = Solution.GetComponent<UnitTestServerTestImpl>();
            return unitTestServer.RemoteChannel;
        }

        public abstract IUnitTestElementsSource MetadataExplorer { get; }

        protected virtual ICollection<IUnitTestElement> GetUnitTestElements(IProject testProject, string assemblyLocation)
        {
            var observer = new TestUnitTestElementObserver();

            var resolver = new DefaultAssemblyResolver();
            resolver.AddPath(FileSystemPath.Parse(assemblyLocation).Directory);

            using (var loader = new MetadataLoader(resolver))
            {
                MetadataExplorer.ExploreProjects(
                    new Dictionary<IProject, FileSystemPath>
                    {
                        {testProject, FileSystemPath.Parse(assemblyLocation)}
                    },
                    loader, observer);
            }
            return observer.Elements;
        }

        protected virtual ITaskRunnerHostController CreateTaskRunnerHostController(IUnitTestLaunchManager launchManager, IUnitTestResultManager resultManager, IUnitTestAgentManager agentManager, TextWriter output, IUnitTestLaunch launch, int port, TestRemoteChannelMessageListener msgListener)
        {
            return new ProcessTaskRunnerHostController(Logger.GetLogger(typeof(UnitTestTaskRunnerTestBase)), launchManager, resultManager, agentManager, launch, port);
        }

        // Hook for a 9.1 workaround
        private static IUnitTestLaunch GetUnitTestLaunch(UnitTestSessionTestImpl session)
        {
            return session;
        }
    }
}