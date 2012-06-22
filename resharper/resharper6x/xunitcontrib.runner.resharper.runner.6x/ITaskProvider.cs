using System.Collections.Generic;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public interface ITaskProvider
    {
        RemoteTask GetTask(string name, string type, string method);
        RemoteTask GetTask(string id);
        IEnumerable<XunitTestMethodTask> MethodTasks { get; }
        bool IsTheory(string name, string type, string method);
    }
}