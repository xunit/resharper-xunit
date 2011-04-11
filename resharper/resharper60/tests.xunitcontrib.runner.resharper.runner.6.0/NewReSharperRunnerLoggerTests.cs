#if false
using System;
using System.Collections.Generic;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests
{
    public class NewReSharperRunnerLoggerTests
    {
        private const string TestAssemblyLocation = "C:\\assembly.dll";
        private const string TestClassTypeName = "Assembly.GroovyType";

        private readonly XunitTestClassTask classTask = new XunitTestClassTask(TestAssemblyLocation, TestClassTypeName, false);
        private readonly FakeRemoteTaskServer fakeRemoteTaskServer = new FakeRemoteTaskServer();
        private readonly ReSharperRunnerLogger logger;

        public NewReSharperRunnerLoggerTests()
        {
            logger = new ReSharperRunnerLogger(fakeRemoteTaskServer, classTask);
        }

        private static Exception GetException(string message)
        {
            try
            {
                throw new InvalidOperationException(message);
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        [Fact]
        public void ClassStartCallsServerTaskStarting()
        {
            logger.ClassStart();

            Assert.Equal(1, fakeRemoteTaskServer.TaskStartingCalls.Count);
            Assert.Equal(classTask, fakeRemoteTaskServer.TaskStartingCalls[0]);
        }

        [Fact]
        public void ClassFailedReportsExceptionAndFinishesTask()
        {
            var exception = GetException("This is the message");

            var message = ExceptionUtility.GetMessage(exception);
            var stackTrace = ExceptionUtility.GetStackTrace(exception);

            string simplifiedMessage;
            var taskExceptions = ExceptionConverter.ConvertExceptions(exception.GetType().FullName, message, stackTrace,
                                                                      out simplifiedMessage);

            logger.ClassStart();
            logger.ClassFailed("className", exception.GetType().FullName, message, stackTrace);
            logger.ClassFinished();

            Assert.Contains(classTask, fakeRemoteTaskServer.TaskStartingCalls);

            Assert.Equal(1, fakeRemoteTaskServer.TaskExceptionCalls.Count);
            AssertTaskExceptionsCalled(taskExceptions, fakeRemoteTaskServer.TaskExceptionCalls[classTask]);

            Assert.Equal(1, fakeRemoteTaskServer.TaskFinishedCalls.Count);
            AssertTaskFinishedCalled(fakeRemoteTaskServer.TaskFinishedCalls[0], classTask, simplifiedMessage,
                                     TaskResult.Exception);
        }

        private static void AssertTaskExceptionsCalled(TaskException[] expectedTaskExceptions, List<TaskException[]> actualTaskExceptions)
        {
            Assert.Equal(expectedTaskExceptions, actualTaskExceptions);
        }


        private static void AssertTaskFinishedCalled(FakeRemoteTaskServer.TaskFinishedParameters taskFinishedCall,
                                                     RemoteTask remoteTask, string message, TaskResult result)
        {
            Assert.Equal(remoteTask, taskFinishedCall.RemoteTask);
            Assert.Equal(message, taskFinishedCall.Message);
            Assert.Equal(result, taskFinishedCall.Result);
        }
    }
}
#endif
