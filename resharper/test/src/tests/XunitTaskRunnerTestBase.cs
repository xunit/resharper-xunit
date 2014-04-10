using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using JetBrains.DataFlow;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.TaskRunnerFramework;
using JetBrains.ReSharper.TestFramework;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestSupportTests;
using JetBrains.Util;
using XunitContrib.Runner.ReSharper.RemoteRunner;
using XunitContrib.Runner.ReSharper.UnitTestProvider;

namespace XunitContrib.Runner.ReSharper.Tests
{
    // Might be better as a property. Can contain environment vars - point
    // to different versions?
    [TestReferences(@"xunit.dll", @"xunit.extensions.dll", Inherits = true)]
    public abstract class XunitTaskRunnerTestBase : UnitTestTaskRunnerTestBase
    {
        private Action<IProjectFile, UnitTestSessionTestImpl, List<IList<UnitTestTask>>, Lifetime> execute;

        protected override string RelativeTestDataPath
        {
            get { return @"Runner\"; }
        }

        protected override RemoteTaskRunnerInfo GetRemoteTaskRunnerInfo()
        {
            return new RemoteTaskRunnerInfo(XunitTaskRunner.RunnerId, typeof(XunitTaskRunner));
        }

        // You normally call DoOneTest, DoSolution, DoTest, etc from a test
        // method. This will run the test, save the output and compare against
        // the gold file. This doesn't work with xunit, which runs tests in an
        // arbitrary order. I'd rather not reorder the output, instead, capture
        // the output and assert over it
        protected override void DoOneTest(string testName)
        {
            DoTestSolution(GetDllName(testName));
        }

        protected void DoOneTestWithStrictOrdering(string testName)
        {
            execute = ExecuteWithGold;
            DoOneTest(testName);
        }

        protected IEnumerable<XElement> DoOneTestWithCapturedOutput(string testName)
        {
            IEnumerable<XElement> messages = null;
            execute = (projectFile, session, sequences, lifetime) =>
            {
                messages = ExecuteWithCapture(session, sequences, lifetime);
            };
            DoOneTest(testName);
            return messages;
        }

        private string GetDllName(string testName)
        {
            // Get dll + cs file (use TestFileExtensionAttribute to use other
            // than .cs). Check existence + file stamps. If missing or out of
            // date, rebuild. Then call DoTestSolution with dll
            var source = GetTestDataFilePath2(testName + Extension);
            var dll = GetTestDataFilePath2(testName + ".dll");

            if (!dll.ExistsFile || source.FileModificationTimeUtc > dll.FileModificationTimeUtc)
            {
                var references = GetReferencedAssemblies().Select(GetTestDataFilePath).ToArray();
                CompileUtil.CompileCs(source, dll, references, false, generatePdb: false);
            }
            return dll.Name;
        }

        protected override void Execute(IProjectFile projectFile, UnitTestSessionTestImpl session, List<IList<UnitTestTask>> sequences, Lifetime lt)
        {
            execute(projectFile, session, sequences, lt);
        }

        private void ExecuteWithGold(IProjectFile projectFile, UnitTestSessionTestImpl session,
            List<IList<UnitTestTask>> sequences, Lifetime lt)
        {
            ExecuteWithGold(projectFile.Location.FullPath, output => Execute(session, sequences, lt, output));
        }

        private IEnumerable<XElement> ExecuteWithCapture(UnitTestSessionTestImpl session,
            List<IList<UnitTestTask>> sequences, Lifetime lt)
        {
            using (var output = new StringWriter())
            {
                Execute(session, sequences, lt, output);
                var messages = output.ToString();
                Console.WriteLine(messages);
                return CaptureMessages(messages);
            }
        }

        private IEnumerable<XElement> CaptureMessages(string output)
        {
            // Now we have to reparse xml fragments. Sheesh.
            var xml = string.Format("<messages>{0}</messages>", output);
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);

            var xDocument = XDocument.Parse(xml);
            return xDocument.Element("messages").Elements();
        }

        protected override IUnitTestMetadataExplorer MetadataExplorer
        {
            get
            {
                return new XunitTestMetadataExplorer(Solution.GetComponent<XunitTestProvider>(),
                    Solution.GetComponent<UnitTestElementFactory>(),
                    Solution.GetComponent<UnitTestingAssemblyLoader>());
            }
        }
    }
}
