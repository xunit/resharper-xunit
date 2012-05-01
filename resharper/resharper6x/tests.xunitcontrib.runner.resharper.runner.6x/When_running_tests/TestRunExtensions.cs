using System.Linq;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public static class TestRunExtensions
    {
        public static Class DefaultClass(this TestRun testRun)
        {
            return testRun.Classes.Single();
        }

        public static XunitTestClassTask DefaultClassTask(this TestRun testRun)
        {
            return testRun.Classes.Single().ClassTask;
        }
    }
}