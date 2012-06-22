using System;
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
                    var task = new XunitTestTheoryTask(methodTask.Id, name);
                    theoryTasks[key] = task;
                    server.CreateDynamicElement(task);
                }

                return theoryTasks[key];
            }

            return methodTask;
        }

        public RemoteTask GetTask(string id)
        {
            return methodTasks.FirstOrDefault(t => t.Id == id);
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
            return methodTasks.First(m => m.ShortName == method);
        }

        private static string GetTheoryKey(string name, string type, string method)
        {
            return type + "." + method + "|" + name;
        }
    }
}