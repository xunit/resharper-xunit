using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Extensions;
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
            // the list of known methods, since the class has RunWithAttribute.
            // This is mimicking the behaviour in the file explorer - we don't
            // look at the methods when it's a RunWith class
            var methodTask = method.Task;

            testRun.Run();

            Messages.OfEqualTask(methodTask).CreateDynamicElement();
            Messages.OfEqualTask(methodTask).TaskStarting();
            Messages.OfEqualTask(methodTask).TaskFinished();
        }

        [Fact]
        public void Should_call_task_started_on_known_dynamic_method_task()
        {
            testClass.DynamicMethodTasksAreKnownFromPreviousRun = true;
            var method = testClass.AddMethod("testMethod", _ => { }, new Parameter[0]);

            var methodTask = method.Task;

            testRun.Run();

            Assert.False(Messages.ForEqualTask(methodTask).Any(tm => tm.Message == ServerMessage.CreateDynamicElement()));
            Messages.OfSameTask(methodTask).TaskStarting();
            Messages.OfSameTask(methodTask).TaskFinished();
        }

        [Fact]
        public void Should_create_dynamic_theories()
        {
            var method = testClass.AddMethod("TestMethod1", _ => { }, new[] { Parameter.Create<int>("value") },
                new TheoryAttribute(), new InlineDataAttribute(12));

            // Make sure the method doesn't have any known theory tasks.
            // If it can't find a known one, it'll have to create a new one
            var theoryTask = method.TheoryTasks[0];
            method.TheoryTasks.Clear();

            Run();

            Messages.OfEqualTask(theoryTask).CreateDynamicElement();
            Messages.OfEqualTask(theoryTask).TaskStarting();
            Messages.OfEqualTask(theoryTask).TaskFinished();
        }

        private class TestClassCommand : ITestClassCommand
        {
            private readonly Xunit.Sdk.TestClassCommand command;

            public TestClassCommand()
            {
                command = new Xunit.Sdk.TestClassCommand();
            }

            public int ChooseNextTest(ICollection<IMethodInfo> testsLeftToRun)
            {
                return 0;
            }

            public Exception ClassFinish()
            {
                return command.ClassFinish();
            }

            public Exception ClassStart()
            {
                return command.ClassStart();
            }

            public IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo testMethod)
            {
                var commands = command.EnumerateTestCommands(testMethod).ToList();
                if (!commands.Any())
                    commands.Add(new FactCommand(testMethod));
                return commands;
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

            public object ObjectUnderTest { get { return command.ObjectUnderTest; } }
            public ITypeInfo TypeUnderTest
            {
                get { return command.TypeUnderTest; }
                set { command.TypeUnderTest = value; }
            }
        }
    }
}