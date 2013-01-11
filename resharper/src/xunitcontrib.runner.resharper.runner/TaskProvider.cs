using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public class TaskProvider
    {
        private readonly IRemoteTaskServer server;
        private readonly IDictionary<string, XunitTestClassTask> classTasks = new Dictionary<string, XunitTestClassTask>();
        private readonly IDictionary<string, IList<XunitTestMethodTask>> methodTasks = new Dictionary<string, IList<XunitTestMethodTask>>();

        
        private readonly IDictionary<string, XunitTestTheoryTask> theoryTasks;

        public TaskProvider(IRemoteTaskServer server)
        {
            this.server = server;
            theoryTasks = new Dictionary<string, XunitTestTheoryTask>();
        }

        public void AddClass(XunitTestClassTask classTask)
        {
            classTasks.Add(classTask.TypeName, classTask);
            methodTasks.Add(classTask.TypeName, new List<XunitTestMethodTask>());
        }

        public void AddMethod(XunitTestClassTask classTask, XunitTestMethodTask methodTask)
        {
            methodTasks[classTask.TypeName].Add(methodTask);
        }

        public RemoteTask GetClassTask(string type)
        {
            return classTasks[type];
        }


        public RemoteTask GetTask(string name, string type, string method)
        {
            var methodTask = GetMethodTask(type, method);
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

        public RemoteTask GetTask(string type, string elementId)
        {
            return methodTasks[type].FirstOrDefault(t => t.ElementId == elementId);
        }

        public bool IsTheory(string name, string type, string method)
        {
            return name != type + "." + method;
        }

        public IEnumerable<XunitTestMethodTask> GetChildren(string type)
        {
            return methodTasks[type];
        }

        private XunitTestMethodTask GetMethodTask(string type, string method)
        {
            return methodTasks[type].First(m => m.MethodName == method);
        }

        private static string GetTheoryKey(string name, string type, string method)
        {
            return type + "." + method + "|" + name;
        }
    }
}