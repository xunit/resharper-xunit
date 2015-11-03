using System.Collections.Generic;
using System.Threading;
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
                    loader, observer, CancellationToken.None);
            }
            return observer.Elements;
        }

        protected ITaskRunnerHostController CreateTaskRunnerHostController(IUnitTestLaunchManager launchManager, IUnitTestResultManager resultManager, IUnitTestAgentManager agentManager, IUnitTestLaunch launch, IUnitTestSessionManager sessionManager, int port)
        {
            return new ProcessTaskRunnerHostController(port, sessionManager, launchManager, resultManager, agentManager, launch, Logger.GetLogger(typeof(UnitTestTaskRunnerTestBase)));
        }
    }
}