using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Xunit;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class FakeExecutorWrapper : IExecutorWrapper
    {
        private readonly TestRun testRun;
        private readonly Func<ITestResult, ITestResult> resultInspector;

        public FakeExecutorWrapper(TestRun testRun, Func<ITestResult, ITestResult> resultInspector)
        {
            this.testRun = testRun;
            this.resultInspector = resultInspector ?? (testResult => testResult);
        }

        public XmlNode RunTests(string type, List<string> methods, Predicate<XmlNode> callback)
        {
            var testClass = testRun.Classes.Single(c => c.Typename == type);
            var methodInfos = (from m in methods
                               select testClass.GetMethod(m)).ToList();
            Predicate<XmlNode> nonNullCallback = node => node == null || callback(node);

            var testClassCommand = TestClassCommandFactory.Make(testClass);
            SetRandomizer(testClassCommand);

            var classResult = TestClassCommandRunner.Execute(testClassCommand,
                                                             methodInfos,
                                                             command => nonNullCallback(command.ToStartXml()),
                                                             result => nonNullCallback(ToXml(resultInspector(result))));

            return ToXml(classResult);
        }

        private static void SetRandomizer(ITestClassCommand testClassCommand)
        {
            var classCommand = testClassCommand as TestClassCommand;
            if (classCommand != null)
                classCommand.Randomizer = new NotVeryRandom();
        }

        private static XmlNode ToXml(ITestResult testResult)
        {
            var doc = new XmlDocument();
            doc.LoadXml("<foo />");
            return testResult.ToXml(doc.ChildNodes[0]);
        }

        public string AssemblyFilename
        {
            get { return testRun.AssemblyLocation; }
        }

        #region

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public XmlNode EnumerateTests()
        {
            throw new NotImplementedException();
        }

        public int GetAssemblyTestCount()
        {
            throw new NotImplementedException();
        }

        public XmlNode RunAssembly(Predicate<XmlNode> callback)
        {
            throw new NotImplementedException();
        }

        public XmlNode RunClass(string type, Predicate<XmlNode> callback)
        {
            throw new NotImplementedException();
        }

        public XmlNode RunTest(string type, string method, Predicate<XmlNode> callback)
        {
            throw new NotImplementedException();
        }

        public string ConfigFilename
        {
            get { throw new NotImplementedException(); }
        }

        public string XunitVersion
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        private class NotVeryRandom : Random
        {
            public override int Next()
            {
                return 0;
            }

            public override int Next(int maxValue)
            {
                return 0;
            }

            public override int Next(int minValue, int maxValue)
            {
                return 0;
            }
        }
    }
}