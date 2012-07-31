using System;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;
using Xunit.Extensions;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    // Calls TestStart multiple times with the same type + method, but the name is the fully qualified name of the method with method parameters
    public class When_a_test_method_contains_theories : SingleClassTestRunContext
    {
        private Method GetDefaultMethodWithTheories()
        {
            return testClass.AddMethod("TestMethod1", _ => { }, new[] {Parameter.Create<int>("value")},
                                       new TheoryAttribute(),
                                       new InlineDataAttribute(12), new InlineDataAttribute(33));
        }

        [Fact]
        public void Should_call_task_starting_once_for_method()
        {
            var method = GetDefaultMethodWithTheories();

            Run();

            var messages = Messages.AssertContainsTaskStarting(method.Task);
            messages.AssertDoesNotContain(TaskMessage.TaskStarting(method.Task));
        }

        [Fact]
        public void Should_call_task_finished_once_for_method()
        {
            var method = GetDefaultMethodWithTheories();

            Run();

            var messages = Messages.AssertContainsTaskFinished(method.Task, string.Empty, TaskResult.Success);
            messages.AssertDoesNotContain(TaskMessage.TaskFinished(method.Task, string.Empty, TaskResult.Success));
        }

        [Fact]
        public void Should_call_task_started_on_dynamic_theory_task()
        {
            var method = testClass.AddMethod("TestMethod1", _ => { }, new[] { Parameter.Create<int>("value") },
                new TheoryAttribute(), new InlineDataAttribute(12));

            Run();

            var theoryTask = new XunitTestTheoryTask(method.Task.ElementId, method.TypeName + "." + method.Name + "(value: 12)");

            var messages = Messages.AssertContainsTaskStarting(theoryTask);
            messages.AssertContainsSuccessfulTaskFinished(theoryTask);
        }

        [Fact]
        public void Should_call_task_succeeded_on_the_method_even_if_any_theories_fail()
        {
            const string expectedMessage = "Broken1";
            var method = testClass.AddMethod("TestMethod1",
                                             values =>
                                                 {
                                                     if ((int) values[0] == 33)
                                                         throw new AssertException(expectedMessage);
                                                 },
                                             new[] { Parameter.Create<int>("value") }, new TheoryAttribute(),
                                             new InlineDataAttribute(12), new InlineDataAttribute(33), new InlineDataAttribute(33));

            Run();

            Messages.AssertContainsSuccessfulTaskFinished(method.Task);
        }

        [Fact]
        public void Should_call_task_output_for_each_theory()
        {
            var method = testClass.AddMethod("TestMethod1", parameters =>
                                                               {
                                                                   var value = (int) parameters[0];
                                                                   if (value == 12)
                                                                       Console.Write("output1");
                                                                   if (value == 33)
                                                                       Console.Write("output2");
                                                               },
                                             new[] { Parameter.Create<int>("value") }, new TheoryAttribute(),
                                             new InlineDataAttribute(12), new InlineDataAttribute(33));

            Run();

            var theoryTask = new XunitTestTheoryTask(method.Task.ElementId, method.TypeName + "." + method.Name + "(value: 12)");
            Messages.AssertContainsTaskOutput(theoryTask, "output1");

            theoryTask = new XunitTestTheoryTask(method.Task.ElementId, method.TypeName + "." + method.Name + "(value: 33)");
            Messages.AssertContainsTaskOutput(theoryTask, "output2");
        }

        [Fact]
        public void Should_call_task_exception_for_each_failing_theory()
        {
            var exception1 = new AssertException("Broken1");
            var exception2 = new AssertException("Broken2");
            var method = testClass.AddMethod("TestMethod1", parameters =>
                                                               {
                                                                   var value = (int) parameters[0];
                                                                   if (value == 12)
                                                                       throw exception1;
                                                                   if (value == 33)
                                                                       throw exception2;
                                                               },
                                             new[] { Parameter.Create<int>("value") }, new TheoryAttribute(),
                                             new InlineDataAttribute(12), new InlineDataAttribute(33));

            Run();

            var theoryTask = new XunitTestTheoryTask(method.Task.ElementId, method.TypeName + "." + method.Name + "(value: 12)");
            Messages.AssertContainsTaskException(theoryTask, exception1);

            theoryTask = new XunitTestTheoryTask(method.Task.ElementId, method.TypeName + "." + method.Name + "(value: 33)");
            Messages.AssertContainsTaskException(theoryTask, exception2);
        }

        [Fact]
        public void Should_call_task_exception_only_on_failing_theories()
        {
            var exception1 = new AssertException("Broken1");
            var method = testClass.AddMethod("TestMethod1", parameters =>
                                                                {
                                                                    var value = (int) parameters[0];
                                                                    if (value == 12)
                                                                        throw exception1;
                                                                },
                                             new[] {Parameter.Create<int>("value")}, new TheoryAttribute(),
                                             new InlineDataAttribute(12), new InlineDataAttribute(33));

            Run();

            var theoryTask = new XunitTestTheoryTask(method.Task.ElementId, method.TypeName + "." + method.Name + "(value: 12)");
            Messages.AssertContainsTaskException(theoryTask, exception1);

            theoryTask = new XunitTestTheoryTask(method.Task.ElementId, method.TypeName + "." + method.Name + "(value: 33)");
            Messages.AssertContainsSuccessfulTaskFinished(theoryTask);
        }
    }
}