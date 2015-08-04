using System.Collections.Generic;
using System.IO;
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

        protected virtual ITaskRunnerHostController CreateTaskRunnerHostController(IUnitTestLaunchManager launchManager, IUnitTestResultManager resultManager, IUnitTestAgentManager agentManager, TextWriter output, IUnitTestLaunch launch, int port, TestRemoteChannelMessageListener msgListener)
        {
            return new ProcessTaskRunnerHostController(Logger.GetLogger(typeof(UnitTestTaskRunnerTestBase)), launchManager, resultManager, agentManager, launch, port, null);
        }

        // 9.1 RTM SDK has a nasty little bug where each test takes 30 seconds to run
        // The issue is that TaskRunnerHostControllerBase.FinishRun calls IUnitTestLaunch.FinishRun, 
        // but the implementation, in UnitTestSessionTestImpl, does nothing. Instead, it's expecting
        // a call to UnitTestSessionTestImpl.Finish, which will run the action we pass to OnFinish
        // and signal the event. Because the event doesn't get signalled, we have to wait 30 seconds
        // for the timeout. This wrapper class implements IUnitTestLaunch by deferring to
        // UnitTestSessionTestImpl, and implementing FinishRun by calling UnitTestSessionTestImpl.Finish
        // https://youtrack.jetbrains.com/issue/RSRP-437172
        private static IUnitTestLaunch GetUnitTestLaunch(UnitTestSessionTestImpl session)
        {
            return session;
        }
    }
}