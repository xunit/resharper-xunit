using System.Collections.Generic;
using System.Linq;
using Microsoft.Silverlight.Testing.Harness;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using Xunit.Sdk;

namespace XunitContrib.Runner.Silverlight.Toolkit
{
    public class TestClassCommandTypeAdapter<TClassUnderTest> : IProvideTestClassCommand, IProvideDynamicTestMethods
    {
        private readonly ITestClassCommand testClassCommand;

        public TestClassCommandTypeAdapter()
        {
            testClassCommand = TestClassCommandFactory.Make(typeof (TClassUnderTest));
        }

        public void ClassStart()
        {
            var exception = testClassCommand.ClassStart();
            if (exception != null)
                throw exception;
        }

        public void ClassFinish()
        {
            var exception = testClassCommand.ClassFinish();
            if (exception != null)
                throw exception;
        }

        public ITestClassCommand TestClassCommand
        {
            get { return testClassCommand; }
        }

        public IEnumerable<ITestMethod> GetDynamicTestMethods()
        {
            return (from methodInfo in testClassCommand.EnumerateTestMethods()
                    from testCommand in ExceptionInterceptingTestCommandFactory.Make(testClassCommand, methodInfo)
                    select new TestMethod(testClassCommand, testCommand, methodInfo)).Cast<ITestMethod>().ToList();
        }
    }
}