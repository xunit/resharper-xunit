using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Runner
{
    [TestFixture("xunit1", Category = "xunit1")]
    [TestFixture("xunit2", Category = "xunit2")]
    public class When_running_multiple_classes : XunitTaskRunnerOutputTestBase
    {
        public When_running_multiple_classes(string environmentId)
            : base(environmentId)
        {
        }

        protected override string GetTestName()
        {
            return "MultipleClasses";
        }

        private TaskId Class1TaskId
        {
            get { return ForTaskOnly("Foo.MultipleClasses1"); }
        }

        private TaskId Class2TaskId
        {
            get { return ForTaskOnly("Foo.MultipleClasses2"); }
        }

        [Test]
        public void Should_notify_start_and_finish_for_each_class()
        {
            AssertMessageOrder(Class1TaskId, TaskAction.Start, TaskAction.Finish);
            AssertMessageOrder(Class2TaskId, TaskAction.Start, TaskAction.Finish);
        }
    }
}