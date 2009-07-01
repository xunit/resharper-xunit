using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper
{
    public class XunitRunner : RemoteTaskRunner
    {
        public XunitRunner(IRemoteTaskServer server)
            : base(server) {}

        public override void ConfigureAppDomain(TaskAppDomainConfiguration configuration) {}

        public override TaskResult Execute(TaskExecutionNode node)
        {
            RemoteTask task = node.RemoteTask;

            if (task is XunitTestClassTask)
                return XunitTestClassRunner.Execute(Server, node, (XunitTestClassTask)task);

            if (task is XunitTestMethodTask)
                return XunitTestMethodRunner.Execute(Server, node, (XunitTestMethodTask)task);

            return TaskResult.Error;
        }

        public override TaskResult Finish(TaskExecutionNode node)
        {
            RemoteTask task = node.RemoteTask;

            if (task is XunitTestClassTask)
                return XunitTestClassRunner.Finish(Server, node, (XunitTestClassTask)task);

            if (task is XunitTestMethodTask)
                return XunitTestMethodRunner.Finish(Server, node, (XunitTestMethodTask)task);

            return TaskResult.Error;
        }

        public override TaskResult Start(TaskExecutionNode node)
        {
            RemoteTask task = node.RemoteTask;

            if (task is XunitTestClassTask)
                return XunitTestClassRunner.Start(Server, node, (XunitTestClassTask)task);

            if (task is XunitTestMethodTask)
                return XunitTestMethodRunner.Start(Server, node, (XunitTestMethodTask)task);

            return TaskResult.Error;
        }
    }
}