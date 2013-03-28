using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public static class SimpleClientControllerFactory
    {
        public static ISimpleClientController Create(IRemoteTaskServer server)
        {
            return new LegacySimpleClientController(server);
        }
    }
}