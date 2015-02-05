using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public static class SimpleRemoteTaskServerFactory
    {
        public static ISimpleRemoteTaskServer Create(IRemoteTaskServer server)
        {
            return new LegacySimpleRemoteTaskServer(server);
        }
    }
}