using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public class NullClientController : ITaskRunnerClientController
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
    }
}