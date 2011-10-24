using System;
using System.Reflection;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public class XunitTaskRunner : RecursiveRemoteTaskRunner
    {
        private readonly ITaskRunnerClientController clientControllerInfo;
        private readonly XunitTestRunner taskRunner;

        public XunitTaskRunner(IRemoteTaskServer server) : base(server)
        {
            clientControllerInfo = CreateClientControllerInfo();

            taskRunner = new XunitTestRunner(server);
        }

        public override TaskResult Start(TaskExecutionNode node)
        {
            var message = clientControllerInfo.BeforeRunStarted();
            ReportClientMessage(message);

            return taskRunner.Start(node);
        }

        public override TaskResult Execute(TaskExecutionNode node)
        {
            // This will not get called, because we derive from RecursiveRemoteTaskRunner
            return TaskResult.Error;
        }

        public override void ExecuteRecursive(TaskExecutionNode node)
        {
            taskRunner.ExecuteRecursive(node);
        }

        public override TaskResult Finish(TaskExecutionNode node)
        {
            var message = clientControllerInfo.AfterRunFinished();
            ReportClientMessage(message);

            return taskRunner.Finish(node);
        }

        private ITaskRunnerClientController CreateClientControllerInfo()
        {
            var info = Server.ClientControllerInfo;
            if (info != null)
            {
                var assembly = Assembly.LoadFrom(info.CodeBase);
                if (assembly != null)
                {
                    var type = assembly.GetType(info.TypeName);
                    if (type != null)
                    {
                        return (ITaskRunnerClientController) Activator.CreateInstance(type);
                    }
                }
            }

            return new NullClientController();
        }

        private void ReportClientMessage(string message)
        {
            if (!string.IsNullOrEmpty(message))
                Server.ClientMessage(message);
        }
    }
}