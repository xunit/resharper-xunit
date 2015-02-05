using System;
using System.Reflection;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public partial class LegacySimpleClientController : ISimpleClientController
    {
        private readonly IRemoteTaskServer server;
        private readonly ITaskRunnerClientController clientController;

        public LegacySimpleClientController(IRemoteTaskServer server)
        {
            this.server = server;
            clientController = CreateClientController();
        }

        public void TaskRunStarting()
        {
            ReportClientMessage(clientController.BeforeRunStarted());
        }

        public void TaskStarting(RemoteTask remoteTask)
        {
            ReportClientMessage(clientController.BeforeTaskStarted(remoteTask));
        }

        public void TaskFinished(RemoteTask remoteTask)
        {
            ReportClientMessage(clientController.AfterTaskFinished(remoteTask));
        }

        public void TaskRunFinished()
        {
            ReportAdditionalInfoToClientController();
            ReportClientMessage(clientController.AfterRunFinished());
        }

        partial void ReportAdditionalInfoToClientController();

        private ITaskRunnerClientController CreateClientController()
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
                        return (ITaskRunnerClientController) Activator.CreateInstance(type);
                    }
                }
            }
            return new NullClientController();
        }

        private void ReportClientMessage(string message)
        {
            if (!string.IsNullOrEmpty(message))
                server.ClientMessage(message);
        }

        private class NullClientController : ITaskRunnerClientController
        {
            public string BeforeRunStarted()
            {
                return null;
            }

            public string AfterRunFinished()
            {
                return null;
            }

            public string BeforeTaskStarted(RemoteTask task)
            {
                return null;
            }

            public string AfterTaskFinished(RemoteTask task)
            {
                return null;
            }

            // ReSharper 7.1
            public void AdditionalControllerInfo(string info)
            {
            }
        }
    }
}