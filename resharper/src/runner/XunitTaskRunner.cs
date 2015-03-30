using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public class XunitTaskRunner : RecursiveRemoteTaskRunner
    {
        public const string RunnerId = "xUnit";

        private readonly TestRunner testRunner;
        private readonly RemoteTaskServer taskServer;

        public XunitTaskRunner(IRemoteTaskServer server)
            : base(server)
        {
            taskServer = new RemoteTaskServer(server, TaskExecutor.Configuration);
            testRunner = new TestRunner(taskServer);
        }

        public override void ExecuteRecursive(TaskExecutionNode node)
        {
            // node is XunitBootstapTask, which we don't care about (see comment on XunitBootstrapTask)
            var assemblyTaskNode = node.Children[0];
            var assemblyTask = (XunitTestAssemblyTask) assemblyTaskNode.RemoteTask;

            taskServer.TaskRunStarting();
            testRunner.Run(assemblyTask, TaskProvider.Create(taskServer, assemblyTaskNode), assemblyTaskNode);
            taskServer.TaskRunFinished();
        }

        public override void Abort()
        {
            taskServer.ShouldContinue = false;
        }
    }
}