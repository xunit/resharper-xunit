using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.DataFlow;
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
            return new FinishRunFixingLaunch(session);
        }

        private class FinishRunFixingLaunch : IUnitTestLaunch
        {
            private readonly IUnitTestLaunch session;

            public FinishRunFixingLaunch(IUnitTestLaunch session)
            {
                this.session = session;
            }

            public void FinishRun(IUnitTestRun run)
            {
                session.Finish();
            }

            #region Boring

            public void PutData<T>(Key<T> key, T value) where T : class
            {
                session.PutData(key, value);
            }

            public T GetData<T>(Key<T> key) where T : class
            {
                return session.GetData(key);
            }

            public IEnumerable<KeyValuePair<object, object>> EnumerateData()
            {
                return session.EnumerateData();
            }

            public IUnitTestRun GetRun(string id)
            {
                return session.GetRun(id);
            }

            public RemoteTask GetRemoteTaskForElement(IUnitTestElement element)
            {
                return session.GetRemoteTaskForElement(element);
            }

            public void Run(IUnitTestElements elements, IEnumerable<IProject> projects, IHostProvider provider,
                            PlatformType automatic,
                            PlatformVersion frameworkVersion)
            {
                session.Run(elements, projects, provider, automatic, frameworkVersion);
            }

            public void TaskStarting(IUnitTestRun run, RemoteTask remoteTask)
            {
                session.TaskStarting(run, remoteTask);
            }

            public void TaskOutput(IUnitTestRun run, RemoteTask remoteTask, string text, TaskOutputType type)
            {
                session.TaskOutput(run, remoteTask, text, type);
            }

            public void TaskException(IUnitTestRun run, RemoteTask remoteTask, TaskException[] exceptions)
            {
                session.TaskException(run, remoteTask, exceptions);
            }

            public void TaskFinished(IUnitTestRun run, RemoteTask remoteTask, string message, TaskResult result)
            {
                session.TaskFinished(run, remoteTask, message, result);
            }

            public void TaskDuration(IUnitTestRun run, RemoteTask remoteTask, TimeSpan duration)
            {
                session.TaskDuration(run, remoteTask, duration);
            }

            public void TaskDiscovered(IUnitTestRun run, RemoteTask task)
            {
                session.TaskDiscovered(run, task);
            }

            public void ShowNotification(string message, string description)
            {
                session.ShowNotification(message, description);
            }

            public void ShowNotification(string message, Action action)
            {
                session.ShowNotification(message, action);
            }

            public void HideHotification()
            {
                session.HideHotification();
            }

            public void Finish()
            {
                session.Finish();
            }

            public void Abort()
            {
                session.Abort();
            }

            public IUnitTestRun CreateRun(RuntimeEnvironment runtimeEnvironment)
            {
                return session.CreateRun(runtimeEnvironment);
            }

            public string Id
            {
                get { return session.Id; }
            }

            public Lifetime Lifetime
            {
                get { return session.Lifetime; }
            }

            public IProperty<UnitTestSessionState> State
            {
                get { return session.State; }
            }

            public IEnumerable<IUnitTestRun> Runs
            {
                get { return session.Runs; }
            }

            public IEnumerable<IUnitTestElement> Elements
            {
                get { return session.Elements; }
            }

            public DateTime DateTimeStarted
            {
                get { return session.DateTimeStarted; }
            }

            public IUnitTestElement LastStartedElement
            {
                get { return session.LastStartedElement; }
            }

            #endregion
        }
    }
}