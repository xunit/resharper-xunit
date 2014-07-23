using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public class TaskProvider
    {
        private readonly RemoteTaskServer server;
        private readonly IDictionary<string, ClassTaskInfo> classTasks;
        private readonly IDictionary<string, IList<MethodTaskInfo>> methodTasks;
        private readonly IDictionary<XunitTestMethodTask, IList<TheoryTaskInfo>> theoryTasks;

        private TaskProvider(RemoteTaskServer server)
        {
            this.server = server;

            classTasks = new Dictionary<string, ClassTaskInfo>();
            methodTasks = new Dictionary<string, IList<MethodTaskInfo>>();
            theoryTasks = new Dictionary<XunitTestMethodTask, IList<TheoryTaskInfo>>();
        }

        public static TaskProvider Create(RemoteTaskServer server, TaskExecutionNode assemblyNode)
        {
            var taskProvider = new TaskProvider(server);
            foreach (var classNode in assemblyNode.Children)
            {
                var classTask = (XunitTestClassTask) classNode.RemoteTask;
                taskProvider.AddClass(classTask);
                foreach (var methodNode in classNode.Children)
                {
                    var methodTask = (XunitTestMethodTask) methodNode.RemoteTask;
                    taskProvider.AddMethod(classTask, methodTask);
                    foreach (var theoryNode in methodNode.Children)
                        taskProvider.AddTheory(methodTask, (XunitTestTheoryTask)theoryNode.RemoteTask);
                }
            }
            return taskProvider;
        }

        private void AddClass(XunitTestClassTask classTask)
        {
            classTasks.Add(classTask.TypeName, new ClassTaskInfo(classTask));
            methodTasks.Add(classTask.TypeName, new List<MethodTaskInfo>());
        }

        private void AddMethod(XunitTestClassTask classTask, XunitTestMethodTask methodTask)
        {
            methodTasks[classTask.TypeName].Add(new MethodTaskInfo(methodTask));
        }

        private void AddTheory(XunitTestMethodTask methodTask, XunitTestTheoryTask theoryTask)
        {
            if (!theoryTasks.ContainsKey(methodTask))
                theoryTasks.Add(methodTask, new List<TheoryTaskInfo>());
            theoryTasks[methodTask].Add(new TheoryTaskInfo(theoryTask));
        }

        public ClassTaskInfo GetClassTask(string type)
        {
            ClassTaskInfo taskInfo;
            classTasks.TryGetValue(type, out taskInfo);
            return taskInfo;
        }

        public MethodTaskInfo GetMethodTask(string type, string method)
        {
            var methodTaskInfo = methodTasks[type].FirstOrDefault(m => m.MethodTask.MethodName == method);
            if (methodTaskInfo == null)
            {
                var classTaskInfo = GetClassTask(type);
                var task = new XunitTestMethodTask(classTaskInfo.ClassTask.ProjectId, type, method, true, true);
                methodTaskInfo = new MethodTaskInfo(task);
                methodTasks[type].Add(methodTaskInfo);
                server.CreateDynamicElement(methodTaskInfo.RemoteTask);
            }
            return methodTaskInfo;
        }

        public TheoryTaskInfo GetTheoryTask(string name, string type, string method)
        {
            if (!IsTheory(name, type, method))
                return null;

            var methodTaskInfo = GetMethodTask(type, method);
            if (!theoryTasks.ContainsKey(methodTaskInfo.MethodTask))
                theoryTasks.Add(methodTaskInfo.MethodTask, new List<TheoryTaskInfo>());

            var shortName = GetTheoryShortName(name, type);
            var theoryTaskInfo = theoryTasks[methodTaskInfo.MethodTask].FirstOrDefault(t => t.TheoryTask.TheoryName == shortName);
            if (theoryTaskInfo == null)
            {
                var task = methodTaskInfo.MethodTask;
                var theoryTask = new XunitTestTheoryTask(task.ProjectId, task.TypeName, task.MethodName, shortName);
                theoryTaskInfo = new TheoryTaskInfo(theoryTask);
                theoryTasks[methodTaskInfo.MethodTask].Add(theoryTaskInfo);
                server.CreateDynamicElement(theoryTaskInfo.RemoteTask);
            }
            return theoryTaskInfo;
        }

        private static string GetTheoryShortName(string name, string type)
        {
            var prefix = type + ".";
            var shortName = name.StartsWith(prefix) ? name.Substring(prefix.Length) : name;
            return DisplayNameUtil.Escape(shortName);
        }

        private static bool IsTheory(string name, string type, string method)
        {
            return name != type + "." + method;
        }

        public IEnumerable<TheoryTaskInfo> GetTheories(MethodTaskInfo methodTaskInfo)
        {
            return theoryTasks[methodTaskInfo.MethodTask];
        }

        public IEnumerable<TaskInfo> GetDescendants(ClassTaskInfo classTaskInfo)
        {
            foreach (var m in methodTasks[classTaskInfo.ClassTask.TypeName])
            {
                IList<TheoryTaskInfo> theories;
                if (theoryTasks.TryGetValue(m.MethodTask, out theories))
                    foreach (var t in theories)
                        yield return t;
                yield return m;
            }
        }
    }
}