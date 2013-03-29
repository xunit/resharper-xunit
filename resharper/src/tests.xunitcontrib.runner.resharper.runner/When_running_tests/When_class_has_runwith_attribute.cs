using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class When_class_has_runwith_attribute : SingleClassTestRunContext
    {
        public When_class_has_runwith_attribute()
        {
            testClass.AddAttribute(new RunWithAttribute(typeof(TestClassCommand)));
        }

        [Fact]
        public void Should_call_task_started_on_new_dynamic_method_task()
        {
            var method = testClass.AddMethod("testMethod", _ => { }, new Parameter[0]);

            // Even though we get a method task from the method, it's not added to
            // the list of known methods
            var methodTask = method.Task;

            testRun.Run();

            Messages.AssertEqualTask(methodTask).CreateDynamicElement();
            Messages.AssertEqualTask(methodTask).TaskStarting();
            Messages.AssertEqualTask(methodTask).TaskFinished();
        }

        private class TestClassCommand : ITestClassCommand
        {
            public int ChooseNextTest(ICollection<IMethodInfo> testsLeftToRun)
            {
                return 0;
            }

            public Exception ClassFinish()
            {
                return null;
            }

            public Exception ClassStart()
            {
                return null;
            }

            public IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo testMethod)
            {
                yield return new FactCommand(testMethod);
            }

            public IEnumerable<IMethodInfo> EnumerateTestMethods()
            {
                return from method in TypeUnderTest.GetMethods()
                       where IsTestMethod(method)
                       select method;
            }

            public bool IsTestMethod(IMethodInfo testMethod)
            {
                return testMethod.Name.StartsWith("test", StringComparison.InvariantCultureIgnoreCase);
            }

            public object ObjectUnderTest { get { return null; } }
            public ITypeInfo TypeUnderTest { get; set; }
        }
    }
}