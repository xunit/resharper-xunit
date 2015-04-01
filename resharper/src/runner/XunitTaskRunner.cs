using JetBrains.ReSharper.TaskRunnerFramework;
using XunitContrib.Runner.ReSharper.RemoteRunner.Tasks;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public class XunitTaskRunner : RecursiveRemoteTaskRunner
    {
        public const string RunnerId = "xUnit";

        private readonly TestRunner testRunner;

        public XunitTaskRunner(IRemoteTaskServer server)
            : base(server)
        {
            testRunner = new TestRunner(server);
        }

        public override void ExecuteRecursive(TaskExecutionNode node)
        {
            // node is XunitBootstapTask, which we don't care about (see comment on XunitBootstrapTask)
            var assemblyTaskNode = node.Children[0];
            var assemblyTask = (XunitTestAssemblyTask) assemblyTaskNode.RemoteTask;

            PopulateRunContext(testRunner.RunContext, assemblyTaskNode);

            testRunner.Run(assemblyTask);
        }

        public override void Abort()
        {
            testRunner.Abort();
        }

        private static void PopulateRunContext(RunContext runContext, TaskExecutionNode assemblyTaskNode)
        {
            foreach (var classNode in assemblyTaskNode.Children)
            {
                runContext.Add((XunitTestClassTask)classNode.RemoteTask);
                foreach (var methodNode in classNode.Children)
                {
                    runContext.Add((XunitTestMethodTask)methodNode.RemoteTask);
                    foreach (var theoryNode in methodNode.Children)
                        runContext.Add((XunitTestTheoryTask) theoryNode.RemoteTask);
                }
            }
        }
    }
}