using System.Collections.Generic;
using Xunit;
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
            foreach (var @class in Classes)
            {
                var logger = new ReSharperRunnerLogger(taskServer, @class.ClassTask, @class.MethodTasks);
                var executor = new FakeExecutorWrapper(this);
                var runner = new TestRunner(executor, logger);

                // TODO: I want to get rid of these calls to ClassStart/ClassFinish
                logger.ClassStart();

                runner.RunTests(@class.Typename, @class.MethodTasks.Select(m => m.ShortName).ToList());

                logger.ClassFinished();
            }
        }

        public IEnumerable<TaskMessage> Messages
        {
            get { return taskServer.Messages.Hide(); }
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