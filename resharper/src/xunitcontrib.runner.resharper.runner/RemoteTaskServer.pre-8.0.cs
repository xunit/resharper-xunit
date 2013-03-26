using System;
using System.Reflection;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public partial class RemoteTaskServer
    {
        private ITaskRunnerClientController clientController = new NullClientController();

        // Removed in 8.0
        partial void TaskExplain(RemoteTask remoteTask, string explanation)
        {
            server.TaskExplain(remoteTask, explanation);
        }

        // Removed in 8.0
        partial void CreateClientController()
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
                        clientController = (ITaskRunnerClientController)Activator.CreateInstance(type);
                    }
                }
            }
        }

        partial void ReportRunStartedToClientContoller()
        {
            ReportClientMessage(clientController.BeforeRunStarted());
        }

        partial void ReportTaskStartingToClientController(RemoteTask remoteTask)
        {
            ReportClientMessage(clientController.BeforeTaskStarted(remoteTask));
        }

        partial void ReportTaskFinishedToClientContoller(RemoteTask remoteFinished)
        {
            ReportClientMessage(clientController.AfterTaskFinished(remoteFinished));
        }

        partial void ReportRunFinishedToClientController()
        {
            ReportAdditionInfoToClientController();
            ReportClientMessage(clientController.AfterRunFinished());
        }

        private void ReportClientMessage(string message)
        {
            if (!string.IsNullOrEmpty(message))
                server.ClientMessage(message);
        }
    }
}