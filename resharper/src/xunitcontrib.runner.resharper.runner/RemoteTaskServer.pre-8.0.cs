using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public partial class RemoteTaskServer
    {
        // Removed in 8.0
        partial void TaskExplain(RemoteTask remoteTask, string explanation)
        {
            server.TaskExplain(remoteTask, explanation);
        }
    }
}