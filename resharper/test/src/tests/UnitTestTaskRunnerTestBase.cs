using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using JetBrains.DataFlow;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Utils;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.TaskRunnerFramework;
using JetBrains.ReSharper.TestFramework;
using JetBrains.ReSharper.TestFramework.Components.UnitTestSupport;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.Strategy;
using JetBrains.ReSharper.UnitTestSupportTests;
using JetBrains.Util;
using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests
{
    // Copy of UnitTestTaskRunnerTestBase in JetBrains.ReSharper.UnitTestSupportTests,
    // which doesn't make MetadataExplorer abstract, and hardcodes it to nunit. It also
    // removes some nunit specific settings in DoTest, and mucks about with Execute to
    // allow capturing output instead of always using ExecuteWithGold
    public abstract class UnitTestTaskRunnerTestBase : BaseTestWithSingleProject
    {
        protected override void DoTest(IProject testProject)
        {
            Lifetimes.Using(lt =>
            {
                ChangeSettingsTemporarily(lt);

                var projectFile = (IProjectFile)testProject.GetSubItems()[0];

                var sessionManager = Solution.GetComponent<IUnitTestSessionManager>();
                var tests = GetUnitTestElements(testProject, projectFile.Location.FullPath);

                Assert.IsNotEmpty(tests, "No tests to run");

                var unitTestLaunchManagerTestImpl = Solution.GetComponent<UnitTestLaunchManagerTestImpl>();
                var session = (UnitTestSessionTestImpl)sessionManager.CreateSession();
                unitTestLaunchManagerTestImpl.AddLaunch(lt, session);

                var sequences = tests.Select(unitTestElement => unitTestElement.GetTaskSequence(EmptyList<IUnitTestElement>.InstanceList, session)).Where(sequence => sequence.Count > 0).ToList();

                Execute(projectFile, session, sequences, lt);
            });
        }

        protected abstract void Execute(IProjectFile projectFile, UnitTestSessionTestImpl session,
            List<IList<UnitTestTask>> sequences, Lifetime lt);

        protected void Execute(UnitTestSessionTestImpl session, List<IList<UnitTestTask>> sequences, Lifetime lt, TextWriter output)
        {
            var msgListener = Solution.GetComponent<TestRemoteChannelMessageListener>();
            msgListener.Output = output;
            session.Sequences = sequences;
            msgListener.Run = session;
            msgListener.Strategy = new OutOfProcessUnitTestRunStrategy(GetRemoteTaskRunnerInfo());

            var runController = CreateTaskRunnerHostController(Solution.GetComponent<IUnitTestLaunchManager>(),
                Solution.GetComponent<IUnitTestAgentManager>(), output, session,
                Solution.GetComponent<UnitTestServer>().PortNumber, msgListener);
            msgListener.RunController = runController;
            var finished = new AutoResetEvent(false);
            session.Run(lt, runController, msgListener.Strategy, () => finished.Set());
            finished.WaitOne(30000);
        }

        protected ICollection<IUnitTestElement> GetUnitTestElements(IProject testProject, string assemblyLocation)
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
                    metadataExplorer.ExploreAssembly(testProject, metadataAssembly, tests.Add, new ManualResetEvent(false));
                }
            }

            return tests;
        }

        protected virtual IEnumerable<string> ProjectReferences
        {
            get { return EmptyList<string>.InstanceList; }
        }

        protected abstract IUnitTestMetadataExplorer MetadataExplorer { get; }
        protected abstract RemoteTaskRunnerInfo GetRemoteTaskRunnerInfo();

        protected virtual ITaskRunnerHostController CreateTaskRunnerHostController(IUnitTestLaunchManager launchManager, IUnitTestAgentManager agentManager, TextWriter output, IUnitTestLaunch launch, int port, TestRemoteChannelMessageListener msgListener)
        {
            return new ProcessTaskRunnerHostController(launchManager, agentManager, launch, port);
        }
    }
}