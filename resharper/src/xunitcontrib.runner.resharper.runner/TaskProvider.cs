using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public class TaskProvider : ITaskProvider
    {
        private readonly IList<XunitTestMethodTask> methodTasks;
        private readonly IRemoteTaskServer server;
        private readonly IDictionary<string, XunitTestTheoryTask> theoryTasks;

        public TaskProvider(IList<XunitTestMethodTask> methodTasks, IRemoteTaskServer server)
        {
            this.methodTasks = methodTasks;
            this.server = server;
            theoryTasks = new Dictionary<string, XunitTestTheoryTask>();
        }

        public RemoteTask GetTask(string name, string type, string method)
        {
            var methodTask = GetMethodTask(method);
            if (IsTheory(name, type, method))
            {
                var key = GetTheoryKey(name, type, method);

                if (!theoryTasks.ContainsKey(key))
                {
                    var shortName = GetTheoryShortName(name, type);
                    var task = new XunitTestTheoryTask(methodTask.ElementId, shortName);
                    theoryTasks[key] = task;
                    server.CreateDynamicElement(task);
                }

                return theoryTasks[key];
            }

            return methodTask;
        }

        private static string GetTheoryShortName(string name, string type)
        {
            var prefix = type + ".";
            return name.StartsWith(prefix) ? name.Substring(prefix.Length) : name;
        }

        public RemoteTask GetTask(string elementId)
        {
            return methodTasks.FirstOrDefault(t => t.ElementId == elementId);
        }

        public bool IsTheory(string name, string type, string method)
        {
            return name != type + "." + method;
        }

        public IEnumerable<XunitTestMethodTask> MethodTasks
        {
            get { return methodTasks; }
        }

        private XunitTestMethodTask GetMethodTask(string method)
        {
            return methodTasks.First(m => m.MethodName == method);
        }

        private static string GetTheoryKey(string name, string type, string method)
        {
            return type + "." + method + "|" + name;
        }
    }
}