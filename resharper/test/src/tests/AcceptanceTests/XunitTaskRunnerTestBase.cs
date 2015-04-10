using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using JetBrains.Application.Components;
using JetBrains.Application.platforms;
using JetBrains.DataFlow;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.impl;
using JetBrains.ReSharper.TaskRunnerFramework;
using JetBrains.ReSharper.TestFramework.Components.UnitTestSupport;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestSupportTests;
using JetBrains.Util;
using XunitContrib.Runner.ReSharper.RemoteRunner;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests
{
    public abstract partial class XunitTaskRunnerTestBase : UnitTestTaskRunnerTestBase
    {
        private System.Action<IProjectFile, UnitTestSessionTestImpl, List<IList<UnitTestTask>>, Lifetime, IUnitTestLaunch> execute;

        protected XunitTaskRunnerTestBase(string environmentId)
        {
            XunitEnvironment = GetAllEnvironments().Single(e => e.Id == environmentId);
        }

        protected IXunitEnvironment XunitEnvironment { get; private set; }

        private static IEnumerable<IXunitEnvironment> GetAllEnvironments()
        {
            // TODO: Add in all old versions of xunit?
            yield return new Xunit1Environment();
            yield return new Xunit2Environment();
        }

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            EnvironmentVariables.SetUp(BaseTestDataPath);
            EnsureReferences();
        }

        public override void TestFixtureTearDown()
        {
            base.TestFixtureTearDown();
            CleanupReferences();
        }

        private void EnsureReferences()
        {
            // Copy the xunit dlls to the current dir
            foreach (var assembly in GetReferencedAssemblies())
            {
                var assemblyPath = FileSystemPath.Parse(assembly);
                var destination = TestDataPath2.CombineWithShortName(assemblyPath.Name);

                if (assemblyPath.IsAbsolute && assemblyPath.ExistsFile && !destination.ExistsFile)
                    assemblyPath.CopyFile(destination, true);
            }
        }

        private void CleanupReferences()
        {
            foreach (var assembly in GetReferencedAssemblies())
            {
                var assemblyPath = FileSystemPath.Parse(assembly);
                var destination = TestDataPath2.CombineWithShortName(assemblyPath.Name);

                if (assemblyPath.IsAbsolute && assemblyPath.ExistsFile && destination.ExistsFile)
                    destination.DeleteFile();
            }
        }

        protected override string RelativeTestDataPath
        {
            get { return @"Runner\"; }
        }

        protected override RemoteTaskRunnerInfo GetRemoteTaskRunnerInfo()
        {
            return new RemoteTaskRunnerInfo(XunitTaskRunner.RunnerId, typeof(XunitTaskRunner));
        }

        protected override IEnumerable<string> GetReferencedAssemblies()
        {
            return XunitEnvironment.GetReferences(GetPlatformID(), FileSystemPath.Empty);
        }

        protected override PlatformID GetPlatformID()
        {
            return XunitEnvironment.GetPlatformID();
        }

        // You normally call DoOneTest, DoSolution, DoTest, etc from a test
        // method. This will run the test, save the output and compare against
        // the gold file. This doesn't work with xunit, which runs tests in an
        // arbitrary order. I'd rather not reorder the output, instead, capture
        // the output and assert over it
        protected override void DoOneTest(string testName)
        {
            DoTestSolution(EnsureTestDll(testName));
        }

        protected void DoOneTestWithStrictOrdering(string testName)
        {
            execute = ExecuteWithGold;
            DoOneTest(testName);
        }

        protected IList<XElement> DoOneTestWithCapturedOutput(string testName)
        {
            IList<XElement> messages = null;
            execute = (projectFile, session, sequences, lifetime, launch) =>
            {
                messages = ExecuteWithCapture(session, sequences, lifetime, launch);
            };
            DoOneTest(testName);
            return messages;
        }

        private string EnsureTestDll(string testName)
        {
            // Get dll + cs file (use TestFileExtensionAttribute to use other
            // than .cs). Check existence + file stamps. If missing or out of
            // date, rebuild. Then call DoTestSolution with dll
            var source = GetTestDataFilePath2(testName + Extension);
            testName = testName.EndsWith(XunitEnvironment.Id) ? testName : testName + "." + XunitEnvironment.Id;
            var dll = GetTestDataFilePath2(testName + ".dll");

            var references = GetReferencedAssemblies().Select(System.Environment.ExpandEnvironmentVariables).ToArray();
            if (!dll.ExistsFile || source.FileModificationTimeUtc > dll.FileModificationTimeUtc || ReferencesAreNewer(references, dll.FileModificationTimeUtc))
            {
                var frameworkDetectionHelper = ShellInstance.GetComponent<IFrameworkDetectionHelper>();
                CompileCs.Compile(frameworkDetectionHelper, source, dll, references, GetPlatformID().Version);
            }
            return dll.Name;
        }

        private static bool ReferencesAreNewer(IEnumerable<string> references, System.DateTime dateTime)
        {
            return references.Any(r =>
            {
                // Ignore GAC references
                var fileSystemPath = FileSystemPath.TryParse(r);
                if (fileSystemPath.IsAbsolute && fileSystemPath.ExistsFile)
                    return fileSystemPath.FileModificationTimeUtc > dateTime;
                return false;
            });
        }

        protected override void Execute(IProjectFile projectFile, UnitTestSessionTestImpl session, List<IList<UnitTestTask>> sequences, Lifetime lt, IUnitTestLaunch launch)
        {
            execute(projectFile, session, sequences, lt, launch);
        }

        private void ExecuteWithGold(IProjectFile projectFile, UnitTestSessionTestImpl session,
            List<IList<UnitTestTask>> sequences, Lifetime lt, IUnitTestLaunch launch)
        {
            ExecuteWithGold(projectFile.Location.FullPath, output =>
            {
                using (var stringWriter = new StringWriter())
                {
                    Execute(session, sequences, lt, stringWriter, launch);

                    // ReSharper 8.2 uses CDATA, but ReSharper 9.0 doesn't. Normalise by removing
                    var text = stringWriter.ToString();
                    text = text.Replace("<![CDATA[", string.Empty).Replace("]]>", string.Empty);
                    output.Write(text);
                }
            });
        }

        private IList<XElement> ExecuteWithCapture(UnitTestSessionTestImpl session,
            List<IList<UnitTestTask>> sequences, Lifetime lt, IUnitTestLaunch launch)
        {
            using (var output = new StringWriter())
            {
                Execute(session, sequences, lt, output, launch);
                var messages = output.ToString();
                System.Console.WriteLine(messages);
                return CaptureMessages(messages);
            }
        }

        private IList<XElement> CaptureMessages(string output)
        {
            // Now we have to reparse xml fragments. Sheesh.
            var xml = string.Format("<messages>{0}</messages>", output);
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);

            var xDocument = XDocument.Parse(xml);
            return xDocument.Element("messages").Elements().ToList();
        }
    }
}
