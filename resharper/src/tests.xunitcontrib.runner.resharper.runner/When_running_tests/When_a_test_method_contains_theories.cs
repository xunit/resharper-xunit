using System;
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

            Messages.OfTask(method.Task).AssertTaskStarting();
        }

        [Fact]
        public void Should_call_task_finished_once_for_method()
        {
            var method = GetDefaultMethodWithTheories();

            Run();

            Messages.OfTask(method.Task).AssertTaskFinishedSuccessfully();
        }

        [Fact]
        public void Should_call_task_started_on_new_dynamic_theory_task()
        {
            var method = testClass.AddMethod("TestMethod1", _ => { }, new[] { Parameter.Create<int>("value") },
                new TheoryAttribute(), new InlineDataAttribute(12));

            // Make sure the method doesn't have any known theory tasks.
            // If it can't find a known one, it'll have to create a new one
            var theoryTask = method.TheoryTasks[0];
            method.TheoryTasks.Clear();

            Run();

            Messages.OfEquivalentTask(theoryTask).AssertCreateDynamicElement();
            Messages.OfEquivalentTask(theoryTask).AssertTaskStarting();
            Messages.OfEquivalentTask(theoryTask).AssertTaskFinishedSuccessfully();
        }

        [Fact]
        public void Should_reuse_existing_theory_tasks()
        {
            var method = testClass.AddMethod("TestMethod1", _ => { }, new[] { Parameter.Create<int>("value") },
                new TheoryAttribute(), new InlineDataAttribute(12));

            Run();

            var theoryTask = method.TheoryTasks[0];

            Messages.OfTask(theoryTask).AssertTaskStarting();
            Messages.OfTask(theoryTask).AssertTaskFinishedSuccessfully();
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

            Messages.OfTask(method.Task).AssertTaskFinishedSuccessfully();
        }

        [Fact]
        public void Should_call_task_output_for_each_theory()
        {
            const string expectedOutput1 = "output1";
            const string expectedOutput2 = "output2";
            var method = testClass.AddMethod("TestMethod1", parameters =>
                                                               {
                                                                   var value = (int) parameters[0];
                                                                   if (value == 12)
                                                                       Console.Write(expectedOutput1);
                                                                   if (value == 33)
                                                                       Console.Write(expectedOutput2);
                                                               },
                                             new[] { Parameter.Create<int>("value") }, new TheoryAttribute(),
                                             new InlineDataAttribute(12), new InlineDataAttribute(33));

            Run();

            var theoryTasks = method.TheoryTasks;

            Messages.OfEquivalentTask(theoryTasks[0]).AssertTaskOutput(expectedOutput1);
            Messages.OfEquivalentTask(theoryTasks[1]).AssertTaskOutput(expectedOutput2);
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

            var theoryTasks = method.TheoryTasks;

            Messages.OfEquivalentTask(theoryTasks[0]).AssertTaskException(exception1);
            Messages.OfEquivalentTask(theoryTasks[1]).AssertTaskException(exception2);
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

            var theoryTasks = method.TheoryTasks;
            Messages.OfEquivalentTask(theoryTasks[0]).AssertTaskException(exception1);
            Messages.OfEquivalentTask(theoryTasks[1]).AssertTaskFinishedSuccessfully();
        }
    }
}