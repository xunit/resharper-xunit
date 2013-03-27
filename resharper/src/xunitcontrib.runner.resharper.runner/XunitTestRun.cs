using System.Linq;
using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public class XunitTestRun
    {
        private readonly RemoteTaskServer server;
        private readonly IExecutorWrapper executor;
        private readonly TaskProvider taskProvider;

        public XunitTestRun(RemoteTaskServer server, IExecutorWrapper executor, TaskProvider taskProvider)
        {
            this.server = server;
            this.executor = executor;
            this.taskProvider = taskProvider;
        }

        public void RunTests()
        {
            var logger = new ReSharperRunnerLogger(server, taskProvider);
            var runner = new Xunit.TestRunner(executor, logger);

            foreach (var className in taskProvider.ClassNames)
            {
                logger.ClassStart(className);
                runner.RunTests(className, taskProvider.GetMethodNames(className).ToList());
                logger.ClassFinished();
            }
        }
    }
}