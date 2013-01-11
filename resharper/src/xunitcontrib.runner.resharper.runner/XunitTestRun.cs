using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public class XunitTestRun
    {
        private readonly IRemoteTaskServer server;
        private readonly IExecutorWrapper executor;
        private readonly TaskProvider taskProvider;

        public XunitTestRun(IRemoteTaskServer server, IExecutorWrapper executor)
        {
            this.server = server;
            this.executor = executor;
            taskProvider = new TaskProvider(server);
        }

        public void AddClass(XunitTestClassTask classTask, IEnumerable<XunitTestMethodTask> methodTasks)
        {
            taskProvider.AddClass(classTask);
            foreach (var methodTask in methodTasks)
                taskProvider.AddMethod(classTask, methodTask);
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