using System;
using Xunit;
using Xunit.Extensions;
using TestResult = Xunit.Sdk.TestResult;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class When_finishing_a_test : SingleClassTestRunContext
    {
        [Fact]
        public void Should_report_duration_for_passing_test()
        {
            var duration = TimeSpan.FromMinutes(12.34);
            var method = testClass.AddPassingTest("Method1");

            SetResultInspectorToUpdateDuration(duration);

            Run();

            Messages.OfTask(method.Task).AssertTaskDuration(duration);
        }

        [Fact]
        public void Should_report_zero_duration_for_really_quick_test()
        {
            var method = testClass.AddPassingTest("Method1");

            SetResultInspectorToUpdateDuration(TimeSpan.Zero);

            Run();

            Messages.OfTask(method.Task).AssertTaskDuration(TimeSpan.Zero);
        }

        [Fact]
        public void Should_report_duration_for_failing_test()
        {
            var duration = TimeSpan.FromMinutes(12.34);
            var method = testClass.AddFailingTest("TestMethod1", new NotImplementedException());

            SetResultInspectorToUpdateDuration(duration);

            Run();

            Messages.OfTask(method.Task).AssertTaskDuration(duration);
        }

        [Fact]
        public void Should_report_duration_for_passing_theory()
        {
            var duration = TimeSpan.FromMinutes(12.34);
            var method = testClass.AddMethod("TestMethod1", _ => { }, new[] { Parameter.Create<int>("value") },
                                             new TheoryAttribute(),
                                             new InlineDataAttribute(12), new InlineDataAttribute(33));
            var theoryTask = method.TheoryTasks[0];

            SetResultInspectorToUpdateDuration(duration);

            Run();

            Messages.OfEquivalentTask(theoryTask).AssertTaskDuration(duration);
        }

        [Fact]
        public void Should_report_duration_for_failing_theory()
        {
            var duration = TimeSpan.FromMinutes(12.34);
            var method = testClass.AddMethod("TestMethod1", _ =>
                                             {
                                                throw new Exception();
                                             },
                                             new[] {Parameter.Create<int>("value")},
                                             new TheoryAttribute(),
                                             new InlineDataAttribute(12), new InlineDataAttribute(33));
            var theoryTask = method.TheoryTasks[0];

            SetResultInspectorToUpdateDuration(duration);

            Run();

            Messages.OfEquivalentTask(theoryTask).AssertTaskDuration(duration);
        }

        [Fact]
        public void Should_report_duration_for_skipped_test()
        {
            var method = testClass.AddSkippedTest("TestMethod1", "Just because");

            Run();

            Messages.OfTask(method.Task).AssertTaskDuration(TimeSpan.Zero);
        }

        [Fact]
        public void Should_propagate_test_durations_to_class()
        {
            var duration = TimeSpan.FromMinutes(12.34);
            testClass.AddPassingTest("Method1");
            testClass.AddPassingTest("Method2");

            SetResultInspectorToUpdateDuration(duration);

            Run();

            Messages.OfTask(testClass.ClassTask).AssertTaskDuration(duration + duration);
        }

        [Fact]
        public void Should_propagate_theory_durations_to_method()
        {
            var duration = TimeSpan.FromMinutes(12.34);
            var method = testClass.AddMethod("TestMethod1", _ =>
            {
                throw new Exception();
            },
                new[] {Parameter.Create<int>("value")},
                new TheoryAttribute(),
                new InlineDataAttribute(12), new InlineDataAttribute(33));

            var theoryTask1 = method.TheoryTasks[0];
            var theoryTask2 = method.TheoryTasks[0];

            SetResultInspectorToUpdateDuration(duration);

            Run();

            Messages.OfEquivalentTask(theoryTask1).AssertTaskDuration(duration);
            Messages.OfEquivalentTask(theoryTask2).AssertTaskDuration(duration);
            Messages.OfEquivalentTask(method.Task).AssertTaskDuration(duration + duration);
        }

        [Fact]
        public void Should_propagate_theory_durations_to_class()
        {
            var duration = TimeSpan.FromMinutes(12.34);
            testClass.AddMethod("TestMethod1", _ =>
            {
                throw new Exception();
            },
                new[] { Parameter.Create<int>("value") },
                new TheoryAttribute(),
                new InlineDataAttribute(12), new InlineDataAttribute(33));

            SetResultInspectorToUpdateDuration(duration);

            Run();

            Messages.OfEquivalentTask(testClass.ClassTask).AssertTaskDuration(duration + duration);
        }

        [Fact]
        public void Should_propagate_theory_and_fact_durations_to_class()
        {
            var duration = TimeSpan.FromMinutes(12.34);
            testClass.AddMethod("TestMethod1", _ =>
            {
                throw new Exception();
            },
                new[] { Parameter.Create<int>("value") },
                new TheoryAttribute(),
                new InlineDataAttribute(12), new InlineDataAttribute(33));
            testClass.AddPassingTest("TestMethod2");

            SetResultInspectorToUpdateDuration(duration);

            Run();

            Messages.OfEquivalentTask(testClass.ClassTask).AssertTaskDuration(duration + duration + duration);
        }

        private void SetResultInspectorToUpdateDuration(TimeSpan duration)
        {
            ResultInspector = result =>
                {
                    var testResult = result as TestResult;
                    if (testResult != null) testResult.ExecutionTime = duration.TotalSeconds;
                    return result;
                };
        }
    }
}