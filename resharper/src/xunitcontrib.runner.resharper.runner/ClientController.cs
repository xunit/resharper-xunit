using System;
using System.Reflection;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    internal partial class ClientController
    {
        private readonly ITaskRunnerClientController clientController;

        public ClientController(IRemoteTaskServer server)
        {
            clientController = CreateClientController(server);
            Server = new ServerWrapper(server, clientController);
        }

        public ServerWrapper Server { get; private set; }

        public void BeforeExecuteRecursive()
        {
            ReportClientMessage(clientController.BeforeRunStarted());
        }

        public void AfterExecuteRecursive()
        {
            ReportAdditionalControllerInfo();
            ReportClientMessage(clientController.AfterRunFinished());
        }

        // ReSharper 7.1
        partial void ReportAdditionalControllerInfo();

        private static ITaskRunnerClientController CreateClientController(IRemoteTaskServer server)
        {
            var info = server.ClientControllerInfo;
            if (info != null)
            {
                var assembly = Assembly.LoadFrom(info.CodeBase);
                if (assembly != null)
                {
                    var type = assembly.GetType(info.TypeName);
                    if (type != null)
                    {
                        return (ITaskRunnerClientController)Activator.CreateInstance(type);
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