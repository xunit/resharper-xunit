using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.Runner
{
    public class When_running_multiple_classes : XunitTaskRunnerOutputTestBase
    {
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