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
        [Fact]
        public void Should_call_task_starting_once_for_method()
        {
            var method = testClass.AddMethod("TestMethod1", _ => { }, new[] {typeof (int)},
                                             new TheoryAttribute(),
                                             new InlineDataAttribute(12), new InlineDataAttribute(33));

            Run();

            var messages = Messages.AssertContainsTaskStarting(method.Task);
            messages.AssertDoesNotContain(TaskMessage.TaskStarting(method.Task));
        }

        [Fact]
        public void Should_call_task_finished_once_for_method()
        {
            var method = testClass.AddMethod("TestMethod1", _ => { }, new[] {typeof (int)},
                                             new TheoryAttribute(),
                                             new InlineDataAttribute(12), new InlineDataAttribute(33));

            Run();

            var messages = Messages.AssertContainsTaskFinished(method.Task, string.Empty, TaskResult.Success);
            messages.AssertDoesNotContain(TaskMessage.TaskFinished(method.Task, string.Empty, TaskResult.Success));
        }

        [Fact]
        public void Should_call_task_failed_on_the_method_if_any_theories_fail()
        {
            const string expectedMessage = "Broken1";
            var method = testClass.AddMethod("TestMethod1",
                                             values =>
                                                 {
                                                     if ((int) values[0] == 33)
                                                         throw new AssertException(expectedMessage);
                                                 },
                                             new[] {typeof (int)}, new TheoryAttribute(),
                                             new InlineDataAttribute(12), new InlineDataAttribute(33), new InlineDataAttribute(33));

            Run();

            Messages.AssertContainsTaskFinished(method.Task, expectedMessage, TaskResult.Exception);
        }

        [Fact]
        public void Should_display_last_reported_exception_on_task_finish()
        {
            const string expectedMessage = "Broken2";
            var method = testClass.AddMethod("TestMehod1", parameters =>
                                                               {
                                                                   var value = (int) parameters[0];
                                                                   if (value == 22)
                                                                       throw new AssertException("Broken1");
                                                                   if (value == 33)
                                                                       throw new AssertException(expectedMessage);
                                                               },
                                             new[] {typeof (int)}, new TheoryAttribute(),
                                             new InlineDataAttribute(12), new InlineDataAttribute(22), new InlineDataAttribute(33), new InlineDataAttribute(55));

            Run();

            Messages.AssertContainsTaskFinished(method.Task, expectedMessage, TaskResult.Exception);
        }

        [Fact]
        public void Should_call_task_output_for_each_theory()
        {
            var method = testClass.AddMethod("TestMehod1", parameters =>
                                                               {
                                                                   var value = (int) parameters[0];
                                                                   if (value == 12)
                                                                       Console.Write("output1");
                                                                   if (value == 33)
                                                                       Console.Write("output2");
                                                               },
                                             new[] {typeof (int)}, new TheoryAttribute(),
                                             new InlineDataAttribute(12), new InlineDataAttribute(33));

            Run();

            Messages.AssertContainsTaskOutput(method.Task, "output1");
            Messages.AssertContainsTaskOutput(method.Task, "output2");
        }

        [Fact]
        public void Should_call_task_exception_for_each_failing_theory()
        {
            var exception1 = new AssertException("Broken1");
            var exception2 = new AssertException("Broken2");
            var method = testClass.AddMethod("TestMehod1", parameters =>
                                                               {
                                                                   var value = (int) parameters[0];
                                                                   if (value == 12)
                                                                       throw exception1;
                                                                   if (value == 33)
                                                                       throw exception2;
                                                               },
                                             new[] {typeof (int)}, new TheoryAttribute(),
                                             new InlineDataAttribute(12), new InlineDataAttribute(33));

            Run();

            Messages.AssertContainsTaskException(method.Task, exception1);
            Messages.AssertContainsTaskException(method.Task, exception2);
        }
    }
}