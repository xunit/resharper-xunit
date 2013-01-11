using System.Collections.Generic;
using System.Linq;

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

        public void Run()
        {
            var taskProvider = new TaskProvider(taskServer);
            foreach (var @class in Classes)
            {
                taskProvider.AddClass(@class.ClassTask);
                foreach (var methodTask in @class.MethodTasks)
                    taskProvider.AddMethod(@class.ClassTask, methodTask);
            }

            var run = new XunitTestRun(taskServer, new FakeExecutorWrapper(this), taskProvider);
            run.RunTests();
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

        public static TestRun SingleClassRun
        {
            get
            {
                var testRun = new TestRun();
                testRun.AddClass("TestsNamespace.TestClass");
                return testRun;
            }
        }
    }
}