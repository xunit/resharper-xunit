using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public class XunitTaskRunner : RecursiveRemoteTaskRunnerBase
    {
        public const string RunnerId = "xUnit";

        private readonly ClientController clientController;
        private readonly TestRunner testRunner;

        public XunitTaskRunner(IRemoteTaskServer server)
            : base(server)
        {
            clientController = new ClientController(server);
            testRunner = new TestRunner(clientController.Server);
        }

        public override void ExecuteRecursive(TaskExecutionNode node)
        {
            clientController.BeforeExecuteRecursive();
            testRunner.ExecuteRecursive(node);
            clientController.AfterExecuteRecursive();
        }
    }
}