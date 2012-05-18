using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class FakeExecutorWrapper : IExecutorWrapper
    {
        private readonly TestRun testRun;

        public FakeExecutorWrapper(TestRun testRun)
        {
            this.testRun = testRun;
        }

        public XmlNode RunTests(string type, List<string> methods, Predicate<XmlNode> callback)
        {
            var testClass = testRun.Classes.Single(c => c.Typename == type);

            if (testClass.InfrastructureException != null)
                throw testClass.InfrastructureException;

            foreach (var loggingStep in testClass.LoggingSteps)
                callback(loggingStep);

            // We don't use the returned xml node. But we could build by concatenating and wrapping
            return testClass.ClassResultAsXml;
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
    }
}