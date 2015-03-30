using JetBrains.ReSharper.TaskRunnerFramework;
using XunitContrib.Runner.ReSharper.RemoteRunner.Tasks;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public class XunitTaskRunner : RecursiveRemoteTaskRunner
    {
        public const string RunnerId = "xUnit";

        private readonly TestRunner testRunner;
        private readonly RunContext runContext;

        public XunitTaskRunner(IRemoteTaskServer server)
            : base(server)
        {
            runContext = new RunContext(server);
            testRunner = new TestRunner(server, runContext);
        }

        public override void ExecuteRecursive(TaskExecutionNode node)
        {
            // node is XunitBootstapTask, which we don't care about (see comment on XunitBootstrapTask)
            var assemblyTaskNode = node.Children[0];
            var assemblyTask = (XunitTestAssemblyTask) assemblyTaskNode.RemoteTask;

            testRunner.Run(assemblyTask, TaskProvider.Create(null, assemblyTaskNode), assemblyTaskNode);
        }

        public override void Abort()
        {
            runContext.Abort();
        }
    }
}