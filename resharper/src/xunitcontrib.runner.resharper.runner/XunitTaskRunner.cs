using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public partial class XunitTaskRunner : RecursiveRemoteTaskRunner
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
            taskServer.TaskRunStarting();
            testRunner.Run((XunitTestAssemblyTask)node.RemoteTask, TaskProvider.Create(taskServer, node));
            taskServer.TaskRunFinished();
        }
    }
}