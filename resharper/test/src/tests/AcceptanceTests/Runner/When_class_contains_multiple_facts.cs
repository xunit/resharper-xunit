using System;
using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Runner
{
    [TestFixture("xunit1", Category = "xunit1")]
    [TestFixture("xunit2", Category = "xunit2")]
    public class When_class_contains_multiple_facts : XunitTaskRunnerOutputTestBase
    {
        public When_class_contains_multiple_facts(string environmentId)
            : base(environmentId)
        {
        }

        protected override string GetTestName()
        {
            return "MultipleFacts";
        }

        private TaskId ClassTaskId
        {
            get { return ForTaskAndChildren("Foo.MultipleFacts"); }
        }

        private TaskId ClassOnlyTaskId
        {
            get { return ForTaskOnly("Foo.MultipleFacts"); }
        }

        private TaskId Method1TaskId
        {
            get { return ForTaskOnly("Foo.MultipleFacts", "TestMethod1"); }
        }

        private TaskId Method2TaskId
        {
            get { return ForTaskOnly("Foo.MultipleFacts", "TestMethod2"); }
        }

        private TaskId FailingMethodTaskId
        {
            get { return ForTaskOnly("Foo.MultipleFacts", "FailingTest"); }
        }

        [Test]
        public void Should_notify_class_starting_just_once()
        {
            AssertContainsStart(ClassOnlyTaskId);
        }

        [Test]
        public void Should_notify_class_finished_just_once()
        {
            AssertContainsFinish(ClassOnlyTaskId, TaskResult.Exception);
        }

        [Test]
        public void Should_notify_class_before_and_after_all_tests()
        {
            var classStart = GetMessageIndex(ClassOnlyTaskId, TaskAction.Start);
            var classFinish = GetMessageIndex(ClassOnlyTaskId, TaskAction.Finish);

            Assert.IsTrue(classFinish > classStart, "Class finish should come after method start");

            var methodStart = GetMessageIndex(Method1TaskId, TaskAction.Start);
            var methodFinish = GetMessageIndex(Method1TaskId, TaskAction.Finish);

            Assert.IsTrue(methodFinish > methodStart, "Method finish should come after method start");

            methodStart = Math.Min(methodStart, GetMessageIndex(Method2TaskId, TaskAction.Start));
            methodFinish = Math.Max(methodFinish, GetMessageIndex(Method2TaskId, TaskAction.Finish));

            Assert.IsTrue(methodFinish > methodStart, "Method finish should come after method start");

            methodStart = Math.Min(methodStart, GetMessageIndex(FailingMethodTaskId, TaskAction.Start));
            methodFinish = Math.Max(methodFinish, GetMessageIndex(FailingMethodTaskId, TaskAction.Finish));

            Assert.IsTrue(methodFinish > methodStart, "Method finish should come after method start");

            Assert.IsTrue(methodStart > classStart, "Class start should come before first method start");
            Assert.IsTrue(classFinish > methodFinish, "Class finished should come after last method finish");
        }

        [Test]
        public void Should_notify_test_starting_for_each_test_method()
        {
            AssertContainsStart(Method1TaskId);
            AssertContainsStart(Method2TaskId);
        }

        [Test]
        public void Should_notify_test_finished_for_each_test_method()
        {
            AssertContainsFinish(Method1TaskId, TaskResult.Success);
            AssertContainsFinish(Method2TaskId, TaskResult.Success);
        }

        [Test]
        public void Should_continue_running_test_after_failing_test_method()
        {
            // Note: Order isn't defined
            AssertContainsFinish(Method1TaskId, TaskResult.Success);
            AssertContainsFinish(FailingMethodTaskId, TaskResult.Exception);
            AssertContainsFinish(Method2TaskId, TaskResult.Success);
        }
    }
}