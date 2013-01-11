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

        private struct Tasks
        {
            public XunitTestClassTask ClassTask;
            public IList<XunitTestMethodTask> MethodTasks;
        }

        private readonly IList<Tasks> tasks = new List<Tasks>();

        public XunitTestRun(IRemoteTaskServer server, IExecutorWrapper executor)
        {
            this.server = server;
            this.executor = executor;
        }

        public void AddClass(XunitTestClassTask classTask, IEnumerable<XunitTestMethodTask> methodTasks)
        {
            tasks.Add(new Tasks
                          {
                              ClassTask = classTask,
                              MethodTasks = methodTasks.ToList()
                          });
        }

        public void RunTests()
        {
            var taskProvider = new TaskProvider(server);
            var logger = new ReSharperRunnerLogger(server, taskProvider);
            var runner = new Xunit.TestRunner(executor, logger);

            foreach (var task in tasks)
            {
                taskProvider.AddClass(task.ClassTask);
                foreach (var methodTask in task.MethodTasks)
                    taskProvider.AddMethod(task.ClassTask, methodTask);

                logger.ClassStart(task.ClassTask.TypeName);
                runner.RunTests(task.ClassTask.TypeName, task.MethodTasks.Select(m => m.MethodName).ToList());
                logger.ClassFinished();
            }
        }
    }
}