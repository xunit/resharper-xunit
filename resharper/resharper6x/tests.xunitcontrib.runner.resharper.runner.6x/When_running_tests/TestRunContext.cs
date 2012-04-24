namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public abstract class TestRunContext
    {
        protected readonly TestClassRun testClass;
        protected readonly FakeRemoteTaskServer taskServer;

        protected TestRunContext()
        {
            testClass = new TestClassRun("TestsNamespace.TestClass");
            taskServer = new FakeRemoteTaskServer();
        }

        protected ReSharperRunnerLogger CreateLogger()
        {
            return new ReSharperRunnerLogger(taskServer, testClass.ClassTask, testClass.MethodTasks);
        }
    }
}