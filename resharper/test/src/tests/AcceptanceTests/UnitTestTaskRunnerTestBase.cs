using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
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

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests
{
    // Copy of UnitTestTaskRunnerTestBase in JetBrains.ReSharper.UnitTestSupportTests,
    // which hardcodes MetadataExplorer to be nunit, and doesn't make it virtual, so 
    // we can't override it (this is fixed in v9). It also removes some nunit specific
    // settings in DoTest, and mucks about with Execute to allow capturing the output
    // instead of always using ExecuteWithGold. And it registers a handler with 
    // TestRemoteChannelMessageListener to remove the path from the "cache-folder"
    // message
    public abstract class UnitTestTaskRunnerTestBase : BaseTestWithSingleProject
    {
        protected override void DoTest(IProject testProject)
        {
            Lifetimes.Using(lt =>
            {
                ChangeSettingsTemporarily(lt);

                RegisterCacheFolderMessageHandler();

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

        // "cache-folder"'s path attribute isn't anonymised, which means consistent test
        // runs are difficult
        private void RegisterCacheFolderMessageHandler()
        {
            var listener = Solution.GetComponent<TestRemoteChannelMessageListener>();
            listener.RegisterPacketHandler(new CacheFolderPacketHandler(listener));
        }

        private class CacheFolderPacketHandler : ITaskRunnerPacketHandler
        {
            private readonly TestRemoteChannelMessageListener listener;

            public CacheFolderPacketHandler(TestRemoteChannelMessageListener listener)
            {
                this.listener = listener;
            }

            public void Accept(XmlElement packet, RemoteChannel remoteChannel)
            {
                packet.RemoveAttribute("path");
                try
                {
                    listener.Output.WriteLine(packet.OuterXml);
                }
                catch
                {
                    // Ignore
                }
            }

            public string PacketName { get { return "cache-folder"; } }
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