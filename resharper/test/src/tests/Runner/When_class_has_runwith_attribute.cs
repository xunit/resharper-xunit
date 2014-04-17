using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.Runner
{
    [TestFixture("xunit1")]
    [TestFixture("xunit2")]
    public class When_class_has_runwith_attribute : XunitTaskRunnerOutputTestBase
    {
        public When_class_has_runwith_attribute(string environmentId)
            : base(environmentId)
        {
        }

        protected override string GetTestName()
        {
            return "HasRunWith";
        }
        
        private TaskId TestMethodOnlyTaskId
        {
            get { return ForTaskOnly("Foo.HasRunWith", "TestMethod1"); }
        }

        [Test]
        public void Should_create_dynamic_task_for_method()
        {
            AssertContainsCreate(TestMethodOnlyTaskId);
        }

        [Test]
        public void Should_call_start_and_finish_on_dynamic_task()
        {
            AssertContainsStart(TestMethodOnlyTaskId);
            AssertContainsFinish(TestMethodOnlyTaskId, TaskResult.Success);
        }

        [Test]
        public void Should_create_dynamic_theories()
        {
            var theoryTaskId = ForTaskOnly("Foo.HasRunWith", "TestMethodWithTheories", "TestMethodWithTheories(value: 42)");
            AssertContainsCreate(theoryTaskId);
        }
    }
}