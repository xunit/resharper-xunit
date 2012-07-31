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
            var run = new XunitTestRun(taskServer, new FakeExecutorWrapper(this));

            foreach (var @class in Classes)
                run.AddClass(@class.ClassTask, @class.MethodTasks);

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