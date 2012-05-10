using System.Collections.Generic;
using Xunit;

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
            foreach (var @class in classes)
            {
                var logger = new ReSharperRunnerLogger(taskServer, @class.ClassTask, @class.MethodTasks);

                // TODO: We should let the logger keep a track of when it's starting and finishing a class
                // The only awkward bits are ExceptionThrown + ClassFailed
                // ClassFailed is sent via ClassResult + XmlLoggingNode when there's a failure
                // ExceptionThrown is called directly
                // SMELL!!!
                logger.ClassStart();

                if (@class.InfrastructureException != null)
                {
                    logger.ExceptionThrown(@class.ClassTask.AssemblyLocation, @class.InfrastructureException);
                }
                else
                {
                    foreach (var node in @class.LoggingSteps)
                        XmlLoggerAdapter.LogNode(node, logger);
                }


                // SMELL!!!!
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