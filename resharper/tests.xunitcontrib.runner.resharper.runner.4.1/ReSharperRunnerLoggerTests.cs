using System;
using System.Collections.Generic;
using JetBrains.ReSharper.TaskRunnerFramework;
using Moq;
using Xunit;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests
{
    // These tests are a bit horrible. They are purely testing interaction
    public class ReSharperRunnerLoggerTests
    {
        private const string TestAssemblyLocation = "C:\\assembly.dll";
        private const string TestClassTypeName = "Assembly.GroovyType";

        private readonly XunitTestClassTask classTask = new XunitTestClassTask(TestAssemblyLocation, TestClassTypeName, false);
        private readonly Mock<IRemoteTaskServer> mockRemoteTaskServer = new Mock<IRemoteTaskServer>(MockBehavior.Strict);
        private readonly ReSharperRunnerLogger logger;

        public ReSharperRunnerLoggerTests()
        {
            logger = new ReSharperRunnerLogger(mockRemoteTaskServer.Object, classTask);
        }

        [Fact]
        public void ClassStartCallsServerTaskStarting()
        {
            mockRemoteTaskServer.Setup(m => m.TaskStarting(classTask)).AtMostOnce().Verifiable();

            logger.ClassStart();

            mockRemoteTaskServer.Verify();
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

            mockRemoteTaskServer.Setup(m => m.TaskStarting(classTask)).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskException(classTask, MatchExceptions(taskExceptions))).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskFinished(classTask, simplifiedMessage, TaskResult.Exception)).AtMostOnce().Verifiable();

            logger.ClassStart();
            logger.ClassFailed("className", exception.GetType().FullName, message, stackTrace);
            logger.ClassFinished();

            mockRemoteTaskServer.Verify();
        }

        [Fact]
        public void ExceptionThrownCallsReportsExceptionAndFinishesClassTask()
        {
            var exception = GetException("This is the message");

            var message = ExceptionUtility.GetMessage(exception);
            var stackTrace = ExceptionUtility.GetStackTrace(exception);

            string simplifiedMessage;
            var taskExceptions = ExceptionConverter.ConvertExceptions(exception.GetType().FullName, message, stackTrace,
                                                                      out simplifiedMessage);

            mockRemoteTaskServer.Setup(m => m.TaskStarting(classTask)).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskException(classTask, MatchExceptions(taskExceptions))).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskFinished(classTask, simplifiedMessage, TaskResult.Exception)).AtMostOnce().Verifiable();

            logger.ClassStart();
            logger.ExceptionThrown("C:\\assembly.dll", exception);
            logger.ClassFinished();

            mockRemoteTaskServer.Verify();
        }

        // The result of the class task has nothing to do with the result of the test methods.
        // It's purely on if the class worked correctly
        private void AddSuccessfulClassExpectations()
        {
            mockRemoteTaskServer.Setup(m => m.TaskStarting(classTask)).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskFinished(classTask, string.Empty, TaskResult.Success)).AtMostOnce().Verifiable();
        }

        [Fact]
        public void SuccessfulTestRunReportedCorrectly()
        {
            const string methodName = "TestMethod";
            const string displayName = "Test Display Name";
            const string expectedOutput = "This is the output of the test";

            var methodTask = new XunitTestMethodTask(TestAssemblyLocation, TestClassTypeName, methodName, false);
            logger.MethodTasks = new List<XunitTestMethodTask> { methodTask };

            AddSuccessfulClassExpectations();
            mockRemoteTaskServer.Setup(m => m.TaskStarting(methodTask)).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskOutput(methodTask, expectedOutput, TaskOutputType.STDOUT)).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskFinished(methodTask, string.Empty, TaskResult.Success)).AtMostOnce().Verifiable();

            logger.ClassStart();
            logger.TestStart(displayName, TestClassTypeName, methodName);
            logger.TestPassed(displayName, TestClassTypeName, methodName, 2.0);
            logger.TestFinished(displayName, TestClassTypeName, methodName, 3.0, expectedOutput);
            logger.ClassFinished();

            mockRemoteTaskServer.Verify();
        }

        [Fact]
        public void SuccessfulTestRunWithNoOutputReportedCorrectly()
        {
            const string methodName = "TestMethod";
            const string displayName = "Test Display Name";

            var methodTask = new XunitTestMethodTask(TestAssemblyLocation, TestClassTypeName, methodName, false);
            logger.MethodTasks = new List<XunitTestMethodTask> { methodTask };

            AddSuccessfulClassExpectations();
            mockRemoteTaskServer.Setup(m => m.TaskStarting(methodTask)).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskFinished(methodTask, string.Empty, TaskResult.Success)).AtMostOnce().Verifiable();

            logger.ClassStart();
            logger.TestStart(displayName, TestClassTypeName, methodName);
            logger.TestPassed(displayName, TestClassTypeName, methodName, 2.0);
            logger.TestFinished(displayName, TestClassTypeName, methodName, 3.0, string.Empty);
            logger.ClassFinished();

            mockRemoteTaskServer.Verify();
        }

        [Fact]
        public void SkippedTestRunReportedCorrectly()
        {
            const string methodName = "TestMethod";
            const string displayName = "Test Display Name";
            const string expectedSkipReason = "this is the skip reason";

            var methodTask = new XunitTestMethodTask(TestAssemblyLocation, TestClassTypeName, methodName, false);
            logger.MethodTasks = new List<XunitTestMethodTask> { methodTask };

            AddSuccessfulClassExpectations();
            mockRemoteTaskServer.Setup(m => m.TaskExplain(methodTask, expectedSkipReason)).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskFinished(methodTask, expectedSkipReason, TaskResult.Skipped)).AtMostOnce().Verifiable();

            logger.ClassStart();
            // TestStart isn't called!
            logger.TestSkipped(displayName, TestClassTypeName, methodName, expectedSkipReason);
            logger.TestFinished(displayName, TestClassTypeName, methodName, 3.0, null);
            logger.ClassFinished();

            mockRemoteTaskServer.Verify();
        }

        [Fact]
        public void FailingTestRunReportedCorrectly()
        {
            const string methodName = "TestMethod";
            const string displayName = "Test Display Name";
            const string expectedOutput = "This is the output of the test";

            var exception = GetException("This is the message");

            var message = ExceptionUtility.GetMessage(exception);
            var stackTrace = ExceptionUtility.GetStackTrace(exception);

            string simplifiedMessage;
            var taskExceptions = ExceptionConverter.ConvertExceptions(exception.GetType().FullName, message, stackTrace,
                                                                      out simplifiedMessage);

            var methodTask = new XunitTestMethodTask(TestAssemblyLocation, TestClassTypeName, methodName, false);
            logger.MethodTasks = new List<XunitTestMethodTask> { methodTask };

            AddSuccessfulClassExpectations();
            mockRemoteTaskServer.Setup(m => m.TaskStarting(methodTask)).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskException(methodTask, MatchExceptions(taskExceptions))).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskOutput(methodTask, expectedOutput, TaskOutputType.STDOUT)).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskFinished(methodTask, simplifiedMessage, TaskResult.Exception)).AtMostOnce().Verifiable();

            logger.ClassStart();
            logger.TestStart(displayName, TestClassTypeName, methodName);
            logger.TestFailed(displayName, TestClassTypeName, methodName, 3.0, exception.GetType().FullName, message,
                              stackTrace);
            logger.TestFinished(displayName, TestClassTypeName, methodName, 3.0, expectedOutput);
            logger.ClassFinished();

            mockRemoteTaskServer.Verify();
        }

        [Fact]
        public void TestRunWithOneClassAndTwoMethodsReportsCorrectly()
        {
            const string methodName1 = "TestMethod";
            const string displayName1 = "Test Display Name";
            const string expectedOutput1 = "This is the output of the test";

            const string methodName2 = "TestMethod2";
            const string displayName2 = "Another Test Display Name";
            const string expectedOutput2 = "This is the output of the second test";

            var methodTask1 = new XunitTestMethodTask(TestAssemblyLocation, TestClassTypeName, methodName1, false);
            var methodTask2 = new XunitTestMethodTask(TestAssemblyLocation, TestClassTypeName, methodName2, false);
            logger.MethodTasks = new List<XunitTestMethodTask> { methodTask1, methodTask2 };

            AddSuccessfulClassExpectations();
            mockRemoteTaskServer.Setup(m => m.TaskStarting(methodTask1)).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskOutput(methodTask1, expectedOutput1, TaskOutputType.STDOUT)).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskFinished(methodTask1, string.Empty, TaskResult.Success)).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskStarting(methodTask2)).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskOutput(methodTask2, expectedOutput2, TaskOutputType.STDOUT)).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskFinished(methodTask2, string.Empty, TaskResult.Success)).AtMostOnce().Verifiable();

            logger.ClassStart();

            logger.TestStart(displayName1, TestClassTypeName, methodName1);
            logger.TestPassed(displayName1, TestClassTypeName, methodName1, 2.0);
            logger.TestFinished(displayName1, TestClassTypeName, methodName1, 3.0, expectedOutput1);

            logger.TestStart(displayName2, TestClassTypeName, methodName2);
            logger.TestPassed(displayName2, TestClassTypeName, methodName2, 2.0);
            logger.TestFinished(displayName2, TestClassTypeName, methodName2, 3.0, expectedOutput2);

            logger.ClassFinished();

            mockRemoteTaskServer.Verify();
        }

        [Fact]
        public void OneClassAndOneSkippedMethodAndOneSuccessfulMethodReportsCorrectly()
        {
            const string methodName1 = "TestMethod";
            const string displayName1 = "Test Display Name";
            const string expectedSkipReason = "This is the skip reason";

            const string methodName2 = "TestMethod2";
            const string displayName2 = "Another Test Display Name";
            const string expectedOutput2 = "This is the output of the second test";

            var methodTask1 = new XunitTestMethodTask(TestAssemblyLocation, TestClassTypeName, methodName1, false);
            var methodTask2 = new XunitTestMethodTask(TestAssemblyLocation, TestClassTypeName, methodName2, false);
            logger.MethodTasks = new List<XunitTestMethodTask> { methodTask1, methodTask2 };

            AddSuccessfulClassExpectations();
            mockRemoteTaskServer.Setup(m => m.TaskExplain(methodTask1, expectedSkipReason)).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskFinished(methodTask1, expectedSkipReason, TaskResult.Skipped)).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskStarting(methodTask2)).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskOutput(methodTask2, expectedOutput2, TaskOutputType.STDOUT)).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskFinished(methodTask2, string.Empty, TaskResult.Success)).AtMostOnce().Verifiable();

            logger.ClassStart();

            logger.TestSkipped(displayName1, TestClassTypeName, methodName1, expectedSkipReason);
            logger.TestFinished(displayName1, TestClassTypeName, methodName1, 3.0, null);

            logger.TestStart(displayName2, TestClassTypeName, methodName2);
            logger.TestPassed(displayName2, TestClassTypeName, methodName2, 2.0);
            logger.TestFinished(displayName2, TestClassTypeName, methodName2, 3.0, expectedOutput2);

            logger.ClassFinished();

            mockRemoteTaskServer.Verify();
        }

        [Fact]
        public void OneClassAndOneSuccessfulMethodAndOneSkippedMethodReportsCorrectly()
        {
            const string methodName1 = "TestMethod";
            const string displayName1 = "Test Display Name";
            const string expectedOutput1 = "This is the output of the second test";

            const string methodName2 = "TestMethod2";
            const string displayName2 = "Another Test Display Name";
            const string expectedSkipReason = "This is the skip reason";

            var methodTask1 = new XunitTestMethodTask(TestAssemblyLocation, TestClassTypeName, methodName1, false);
            var methodTask2 = new XunitTestMethodTask(TestAssemblyLocation, TestClassTypeName, methodName2, false);
            logger.MethodTasks = new List<XunitTestMethodTask> { methodTask1, methodTask2 };

            AddSuccessfulClassExpectations();
            mockRemoteTaskServer.Setup(m => m.TaskStarting(methodTask1)).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskOutput(methodTask1, expectedOutput1, TaskOutputType.STDOUT)).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskFinished(methodTask1, string.Empty, TaskResult.Success)).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskExplain(methodTask2, expectedSkipReason)).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskFinished(methodTask2, expectedSkipReason, TaskResult.Skipped)).AtMostOnce().Verifiable();

            logger.ClassStart();

            logger.TestStart(displayName1, TestClassTypeName, methodName1);
            logger.TestPassed(displayName1, TestClassTypeName, methodName1, 2.0);
            logger.TestFinished(displayName1, TestClassTypeName, methodName1, 3.0, expectedOutput1);

            logger.TestSkipped(displayName2, TestClassTypeName, methodName2, expectedSkipReason);
            logger.TestFinished(displayName2, TestClassTypeName, methodName2, 3.0, null);

            logger.ClassFinished();

            mockRemoteTaskServer.Verify();
        }

        [Fact]
        public void MultipleTestRowsCallTestLoggerMultipleTimesForSameMethod()
        {
            const string methodName = "TestMethod";
            const string displayName1 = "Test Row 1";
            const string displayName2 = "Test Row 2";
            const string expectedOutput1 = "This is the output of the first test row";
            const string expectedOutput2 = "This is the output of the second test row";

            var methodTask = new XunitTestMethodTask(TestAssemblyLocation, TestClassTypeName, methodName, false);
            logger.MethodTasks = new List<XunitTestMethodTask> { methodTask };

            AddSuccessfulClassExpectations();
            mockRemoteTaskServer.Setup(m => m.TaskStarting(methodTask)).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskOutput(methodTask, expectedOutput1, TaskOutputType.STDOUT)).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskOutput(methodTask, expectedOutput2, TaskOutputType.STDOUT)).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskFinished(methodTask, string.Empty, TaskResult.Success)).AtMostOnce().Verifiable();

            logger.ClassStart();

            logger.TestStart(displayName1, TestClassTypeName, methodName);
            logger.TestPassed(displayName1, TestClassTypeName, methodName, 2.0);
            logger.TestFinished(displayName1, TestClassTypeName, methodName, 3.0, expectedOutput1);

            logger.TestStart(displayName2, TestClassTypeName, methodName);
            logger.TestPassed(displayName2, TestClassTypeName, methodName, 2.0);
            logger.TestFinished(displayName2, TestClassTypeName, methodName, 3.0, expectedOutput2);

            logger.ClassFinished();

            mockRemoteTaskServer.Verify();
        }

        [Fact]
        public void MethodTaskFinishesWithExceptionTaskResultWhenFirstTestRowFails()
        {
            const string methodName = "TestMethod";
            const string displayName1 = "Test Row 1";
            const string displayName2 = "Test Row 2";
            const string expectedOutput1 = "This is the output of the first test row";
            const string expectedOutput2 = "This is the output of the second test row";

            var exception = GetException("This is the message");

            var message = ExceptionUtility.GetMessage(exception);
            var stackTrace = ExceptionUtility.GetStackTrace(exception);

            string simplifiedMessage;
            var taskExceptions = ExceptionConverter.ConvertExceptions(exception.GetType().FullName, message, stackTrace,
                                                                              out simplifiedMessage);

            var methodTask = new XunitTestMethodTask(TestAssemblyLocation, TestClassTypeName, methodName, false);
            logger.MethodTasks = new List<XunitTestMethodTask> { methodTask };

            AddSuccessfulClassExpectations();
            mockRemoteTaskServer.Setup(m => m.TaskStarting(methodTask)).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskOutput(methodTask, expectedOutput1, TaskOutputType.STDOUT)).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskException(methodTask, MatchExceptions(taskExceptions))).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskOutput(methodTask, expectedOutput2, TaskOutputType.STDOUT)).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskFinished(methodTask, simplifiedMessage, TaskResult.Exception)).AtMostOnce().Verifiable();

            logger.ClassStart();

            logger.TestStart(displayName1, TestClassTypeName, methodName);
            logger.TestFailed(displayName1, TestClassTypeName, methodName, 3.0, exception.GetType().FullName, message,
                              stackTrace);
            logger.TestFinished(displayName1, TestClassTypeName, methodName, 3.0, expectedOutput1);

            logger.TestStart(displayName2, TestClassTypeName, methodName);
            logger.TestPassed(displayName2, TestClassTypeName, methodName, 2.0);
            logger.TestFinished(displayName2, TestClassTypeName, methodName, 3.0, expectedOutput2);

            logger.ClassFinished();

            mockRemoteTaskServer.Verify();
        }

        [Fact]
        public void MethodTaskFinishesWithExceptionTaskResultWhenSecondTestRowFails()
        {
            const string methodName = "TestMethod";
            const string displayName1 = "Test Row 1";
            const string displayName2 = "Test Row 2";
            const string expectedOutput1 = "This is the output of the first test row";
            const string expectedOutput2 = "This is the output of the second test row";

            var exception = GetException("This is the message");

            var message = ExceptionUtility.GetMessage(exception);
            var stackTrace = ExceptionUtility.GetStackTrace(exception);

            string simplifiedMessage;
            var taskExceptions = ExceptionConverter.ConvertExceptions(exception.GetType().FullName, message, stackTrace,
                                                                              out simplifiedMessage);

            var methodTask = new XunitTestMethodTask(TestAssemblyLocation, TestClassTypeName, methodName, false);
            logger.MethodTasks = new List<XunitTestMethodTask> { methodTask };

            AddSuccessfulClassExpectations();
            mockRemoteTaskServer.Setup(m => m.TaskStarting(methodTask)).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskOutput(methodTask, expectedOutput1, TaskOutputType.STDOUT)).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskException(methodTask, MatchExceptions(taskExceptions))).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskOutput(methodTask, expectedOutput2, TaskOutputType.STDOUT)).AtMostOnce().Verifiable();
            mockRemoteTaskServer.Setup(m => m.TaskFinished(methodTask, simplifiedMessage, TaskResult.Exception)).AtMostOnce().Verifiable();

            logger.ClassStart();

            logger.TestStart(displayName1, TestClassTypeName, methodName);
            logger.TestPassed(displayName1, TestClassTypeName, methodName, 2.0);
            logger.TestFinished(displayName1, TestClassTypeName, methodName, 3.0, expectedOutput1);

            logger.TestStart(displayName2, TestClassTypeName, methodName);
            logger.TestFailed(displayName2, TestClassTypeName, methodName, 3.0, exception.GetType().FullName, message,
                              stackTrace);
            logger.TestFinished(displayName2, TestClassTypeName, methodName, 3.0, expectedOutput2);

            logger.ClassFinished();

            mockRemoteTaskServer.Verify();
        }

        // If more than one test row, show display names in task progress
        // If more than one test row, output display names
        // If more than one test row, output is prefixed with display name
        // If more than one test row, exception message contains display name
        // If more than one test row + multiple exceptions, each exception set includes display name
        // If more than one test row + exceptions progress message contains "one or more theory tests failed"

        private static TaskException[] MatchExceptions(TaskException[] taskExceptions)
        {
            return Match<TaskException[]>.Create(new TaskExceptionMatcher(taskExceptions).Match);
        }

        private class TaskExceptionMatcher
        {
            private readonly TaskException[] expectedExceptions;

            public TaskExceptionMatcher(TaskException[] expectedExceptions)
            {
                this.expectedExceptions = expectedExceptions;
            }

            public bool Match(TaskException[] actualExceptions)
            {
                if (expectedExceptions.Length != actualExceptions.Length)
                    return false;

                for (int i = 0; i < expectedExceptions.Length; i++)
                {
                    if (expectedExceptions[i].Type != actualExceptions[0].Type
                        && expectedExceptions[i].Message != actualExceptions[i].Message
                        && expectedExceptions[i].StackTrace != actualExceptions[i].StackTrace)
                    {
                        return false;
                    }
                }

                return true;
            }
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
    }
}