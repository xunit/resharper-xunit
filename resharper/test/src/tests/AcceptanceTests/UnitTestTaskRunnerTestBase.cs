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
using JetBrains.ReSharper.UnitTestExplorer;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.Strategy;
using JetBrains.Util;
using JetBrains.Util.Logging;
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

            public void Accept(RemoteTask remoteTask, IDictionary<string, string> attributes, XmlReader reader, IRemoteChannel remoteChannel)
            {
                attributes.Remove("path");
                try
                {
                    var attributesString = string.Empty;
                    if (attributes != null)
                        attributesString = " " + attributes.Select(pair => pair.Key + "=\"" + pair.Value + "\"").Join(" ");
                    var xmldoc = new XmlDocument();
                    xmldoc.LoadXml(string.Format(remoteTask != null ? "<{0}{1}><task/></{0}>" : "<{0}{1}/>", PacketName, attributesString));
                    var task = (XmlElement)xmldoc.DocumentElement.FirstChild;
                    remoteTask.ToXml(task);
                    string xml;
                    bool state;
#pragma warning disable 665
                    while (!string.IsNullOrEmpty(xml = (state = reader.IsStartElement()) ? reader.ReadOuterXml() : reader.ReadString()))
#pragma warning restore 665
                    {
                        if (state)
                        {
                            var doc = new XmlDocument();
                            doc.LoadXml(xml);
                            var message = doc.DocumentElement;
                            if (message != null)
                                xmldoc.DocumentElement.AppendChild(xmldoc.ImportNode(message, true));
                        }
                        else
                        {
                            xmldoc.DocumentElement.AppendChild(xmldoc.CreateTextNode(xml));
                        }
                    }

                    listener.Output.WriteLine(xmldoc.OuterXml);
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

            var unitTestServer = Solution.GetComponent<UnitTestServerTestImpl>();
            var runController = CreateTaskRunnerHostController(Solution.GetComponent<IUnitTestLaunchManager>(), Solution.GetComponent<IUnitTestResultManager>(), Solution.GetComponent<IUnitTestAgentManager>(), output, session, unitTestServer.PortNumber, msgListener);
            msgListener.RunController = runController;
            var finished = new AutoResetEvent(false);
            session.Run(lt, runController, msgListener.Strategy, () => finished.Set());
            finished.WaitOne(30000);
        }

        protected virtual ICollection<IUnitTestElement> GetUnitTestElements(IProject testProject,
                                                                            string assemblyLocation)
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

        protected virtual IEnumerable<string> ProjectReferences
        {
            get { return EmptyList<string>.InstanceList; }
        }

        public abstract IUnitTestElementsSource MetadataExplorer { get; }
        protected abstract RemoteTaskRunnerInfo GetRemoteTaskRunnerInfo();

        protected virtual ITaskRunnerHostController CreateTaskRunnerHostController(IUnitTestLaunchManager launchManager, IUnitTestResultManager resultManager, IUnitTestAgentManager agentManager, TextWriter output, IUnitTestLaunch launch, int port, TestRemoteChannelMessageListener msgListener)
        {
            return new ProcessTaskRunnerHostController(Logger.GetLogger(typeof(UnitTestTaskRunnerTestBase)), launchManager, resultManager, agentManager, launch, port);
        }
    }

    internal class TestUnitTestElementObserver : IUnitTestElementsObserver
    {
        private readonly OrderedHashSet<IUnitTestElement> myElements = new OrderedHashSet<IUnitTestElement>();

        public ICollection<IUnitTestElement> Elements
        {
            get { return myElements; }
        }

        public void OnUnitTestElement(IUnitTestElement element)
        {
            myElements.Add(element);
        }

        public void OnUnitTestElementDisposition(UnitTestElementDisposition disposition)
        {
            myElements.Add(disposition.UnitTestElement);
            if (disposition.SubElements != null)
                foreach (var element in disposition.SubElements)
                    myElements.Add(element);
        }

        public void OnUnitTestElementChanged(IUnitTestElement element)
        {
        }

        public void OnCompleted()
        {
        }

        public IEnumerable<UnitTestElementDisposition> Dispositions { get; private set; }
    }
}