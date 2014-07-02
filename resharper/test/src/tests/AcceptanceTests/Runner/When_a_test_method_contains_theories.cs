using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Runner
{
    [TestFixture("xunit1", Category = "xunit1")]
    [TestFixture("xunit2", Category = "xunit2")]
    public class When_a_test_method_contains_theories : XunitTaskRunnerOutputTestBase
    {
        public When_a_test_method_contains_theories(string environment)
            : base(environment)
        {
        }

        protected override string GetTestName()
        {
            return "Theories";
        }

        private TaskId SuccessfulMethodOnlyTaskId
        {
            get { return ForTaskOnly("Foo.Theory", "SuccessfulMethod"); }
        }

        private TaskId SuccessfulMethodTaskId
        {
            get { return ForTaskAndChildren("Foo.Theory", "SuccessfulMethod"); }
        }

        private TaskId SuccessfulTheoryTaskId
        {
            get
            {
                return ForTaskAndChildren("Foo.Theory", "SuccessfulMethod",
                    "SuccessfulMethod(value: 42)");
            }
        }

        private TaskId FailingClassId
        {
            get { return ForTaskOnly("Foo.FailingTheory"); }
        }

        private TaskId FailingMethodOnlyTaskId
        {
            get { return ForTaskOnly("Foo.FailingTheory", "FailingMethod"); }
        }

        private TaskId FailingTheoryTaskId
        {
            get
            {
                return ForTaskAndChildren("Foo.FailingTheory", "FailingMethod",
                    "FailingMethod(value: 42)");
            }
        }

        [Test]
        public void Should_call_task_starting_once_for_method()
        {
            AssertContainsStart(SuccessfulMethodOnlyTaskId);
        }

        [Test]
        public void Should_call_task_finished_once_for_method()
        {
            AssertContainsFinish(SuccessfulMethodOnlyTaskId, TaskResult.Success);
        }

        [Test]
        public void Should_create_dynamic_theory_task()
        {
            AssertContainsCreate(SuccessfulTheoryTaskId);
        }

        [Test]
        public void Should_call_theory_between_start_and_finish_of_method()
        {
            AssertMessageOrder(SuccessfulMethodTaskId,
                TaskAction.Start,           // Method
                    TaskAction.Create,      // Theory
                    TaskAction.Start,       // Theory
                    TaskAction.Finish,      // Theory
                TaskAction.Finish);
        }

        [Test]
        public void Should_call_task_exception_for_failing_theory()
        {
            AssertContainsException(FailingTheoryTaskId, "System.InvalidOperationException",
                "Broken", "at Foo.FailingTheory.FailingMethod(Int32 value)");
        }

        [Test]
        public void Should_fail_method_if_any_theories_fail()
        {
            AssertContainsFailingChildren(FailingMethodOnlyTaskId);
        }

        [Test]
        public void Should_fail_class_if_any_theories_fail()
        {
            AssertContainsFailingChildren(FailingClassId);
        }
    }
}