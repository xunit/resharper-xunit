using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class TestRun
    {
        private readonly IList<Class> classes;
        private readonly FakeRemoteTaskServer taskServer;

        public TestRun()
        {
            classes = new List<Class>();
            taskServer = new FakeRemoteTaskServer();
        }

        public void Run(Func<ITestResult, ITestResult> resultInspector = null)
        {
            var taskProvider = TaskProvider.Create(new RemoteTaskServer(taskServer, null), CreateTaskNodes());
            var run = new XunitTestRun(new RemoteTaskServer(taskServer, null), new FakeExecutorWrapper(this, resultInspector), taskProvider);
            run.RunTests();
        }

        private TaskExecutionNode CreateTaskNodes()
        {
            var assemblyNode = new TaskExecutionNode(null, null);
            foreach (var @class in Classes)
                assemblyNode.Children.Add(CreateClassNode(assemblyNode, @class));
            return assemblyNode;
        }

        private TaskExecutionNode CreateClassNode(TaskExecutionNode assemblyNode, Class @class)
        {
            var classNode = new TaskExecutionNode(assemblyNode, @class.ClassTask);

            if (ShouldAddMethods(@class))
            {
                foreach (var method in @class.Methods)
                    classNode.Children.Add(CreateMethodNode(classNode, method));
            }
            return classNode;
        }

        private static bool ShouldAddMethods(Class @class)
        {
            // We don't look for methods if it has the RunWith attribute, so don't
            // add them now, unless we've seen them in a previous run
            return !TypeUtility.HasRunWith(@class) || @class.DynamicMethodTasksAreKnownFromPreviousRun;
        }

        private TaskExecutionNode CreateMethodNode(TaskExecutionNode classNode, Method method)
        {
            var methodNode = new TaskExecutionNode(classNode, method.Task);
            foreach (var theoryTask in method.TheoryTasks)
                methodNode.Children.Add(new TaskExecutionNode(methodNode, theoryTask));
            return methodNode;
        }

        public IEnumerable<TaskMessage> Messages
        {
            get { return taskServer.Messages; }
        }

        public IEnumerable<Class> Classes
        {
            get { return classes; }
        }

        public string AssemblyLocation
        {
            get { return classes.First().AssemblyLocation; }
        }

        public Class AddClass(string typeName, string assemblyLocation = "assembly1.dll")
        {
            var @class = new Class(typeName, assemblyLocation);
            classes.Add(@class);
            return @class;
        }

        public static TestRun GetSingleClassRun()
        {
            var testRun = new TestRun();
            testRun.AddClass("TestsNamespace.TestClass");
            return testRun;
        }
    }
}