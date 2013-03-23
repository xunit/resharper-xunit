using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public partial class XunitTaskRunner : RecursiveRemoteTaskRunner
    {
        public const string RunnerId = "xUnit";

        private readonly ClientController clientController;
        private readonly TestRunner testRunner;
        private readonly RemoteTaskServer taskServer;

        public XunitTaskRunner(IRemoteTaskServer server)
            : base(server)
        {
            clientController = new ClientController(server);
            taskServer = new RemoteTaskServer(server, TaskExecutor.Configuration);
            testRunner = new TestRunner(taskServer);
        }

        public override void ExecuteRecursive(TaskExecutionNode node)
        {
            clientController.BeforeExecuteRecursive();
            testRunner.Run((XunitTestAssemblyTask)node.RemoteTask, TaskProvider.Create(taskServer, node));
            clientController.AfterExecuteRecursive();
        }
    }
}