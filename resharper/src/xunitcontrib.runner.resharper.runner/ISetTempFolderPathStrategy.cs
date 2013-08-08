using System.Reflection;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public interface ISetTempFolderPathStrategy
    {
        void SetTempFolderPath(string path);
        void TestRunFinished();
    }

    public static class SetTempFolderPathStrategyFactory
    {
        public static ISetTempFolderPathStrategy Create(IRemoteTaskServer server)
        {
            var version = Assembly.GetEntryAssembly().GetName().Version;
            // TODO: Handle this better for 8.0.1. Need to know what version 8.0.1 will be
            // I'm betting on 8.0.1000-ish
            if (version.Major == 8 && version.Minor == 0)
                return new BrokenSetTempFolderPathStrategy(server);
            return new SetTempFolderPathStrategy(server);
        }
    }

    public class SetTempFolderPathStrategy : ISetTempFolderPathStrategy
    {
        private readonly IRemoteTaskServer server;

        public SetTempFolderPathStrategy(IRemoteTaskServer server)
        {
            this.server = server;
        }

        public void SetTempFolderPath(string path)
        {
            server.SetTempFolderPath(path);
        }

        public void TestRunFinished()
        {
        }
    }

    // ReSharper 8.0 RTM starts deleting the cache folder as soon as you call
    // IRemoteTaskServer.SetTempFolderPath. It has to be called at the end of
    // the run, but that will leak temp folders if the user aborts before the
    // end of the run
    public class BrokenSetTempFolderPathStrategy : ISetTempFolderPathStrategy
    {
        private readonly IRemoteTaskServer server;
        private string tempFolderPath;

        public BrokenSetTempFolderPathStrategy(IRemoteTaskServer server)
        {
            this.server = server;
        }

        public void SetTempFolderPath(string path)
        {
            tempFolderPath = path;
        }

        public void TestRunFinished()
        {
            if (!string.IsNullOrEmpty(tempFolderPath))
                server.SetTempFolderPath(tempFolderPath);
        }
    }
}