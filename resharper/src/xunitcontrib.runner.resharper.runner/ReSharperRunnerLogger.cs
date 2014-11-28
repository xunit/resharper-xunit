using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.TaskRunnerFramework;
using JetBrains.Util;
using Xunit;
using Xunit.Abstractions;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public class ReSharperRunnerLogger : TestMessageVisitor<ITestAssemblyFinished>
    {
        private const string OneOrMoreChildTestsFailedMessage = "One or more child tests failed";

        private readonly RemoteTaskServer server;
        private readonly TaskProvider taskProvider;

        public ReSharperRunnerLogger(RemoteTaskServer server, TaskProvider taskProvider)
        {
            this.server = server;
            this.taskProvider = taskProvider;
        }

        protected override bool Visit(ITestClassStarting testClassStarting)
        {
            var classTaskInfo = taskProvider.GetClassTask(testClassStarting.TestClass.Class.Name);
            TaskStarting(classTaskInfo);
            return server.ShouldContinue;
        }

        // Called for xunit1:
        // 1. Catastrophic error, i.e. the test runner throws. The only expected exception
        //    is for ambiguous method names. Test run is aborted
        // 2. class failure, i.e. fixture's ctor or dispose throws. Test methods are not run
        //    if the fixture's ctor throws. Not called for exceptions in class ctor/dispose
        //    (these are reported as failing test methods, as the class is created for each
        //     test method)
        // Not called for xunit2
        protected override bool Visit(IErrorMessage error)
        {
            // TODO: This is assuming a lot...
            var testClasses = from testCase in error.TestCases
                select testCase.TestMethod.TestClass.Class.Name;

            var taskInfo = taskProvider.GetClassTask(testClasses.First());

            string message;
            var exceptions = ConvertExceptions(error, out message);

            if (taskInfo.RemoteTask is XunitTestClassTask)
            {
                var classTaskInfo = taskInfo;
                var methodMessage = string.Format("Class failed in {0}", classTaskInfo.ClassTask.TypeName);

                var methodExceptions = new[] { new TaskException(null, methodMessage, null) };
                foreach (var task in taskProvider.GetDescendants(classTaskInfo))
                {
                    server.TaskException(task.RemoteTask, methodExceptions);
                    server.TaskFinished(task.RemoteTask, methodMessage, TaskResult.Error, 0);
                }
            }

            taskInfo.Result = TaskResult.Exception;
            taskInfo.Message = message;

            server.TaskException(taskInfo.RemoteTask, exceptions);

            TaskFinished(taskInfo, taskInfo.Message, taskInfo.Result, 0);

            return server.ShouldContinue;
        }

        protected override bool Visit(ITestAssemblyCleanupFailure testAssemblyCleanupFailure)
        {
            Console.WriteLine("ITestAssemblyCleanupFailure");

            // TODO: We can do nothing about this. Report on the root node?
            return server.ShouldContinue;
        }

        protected override bool Visit(ITestCollectionCleanupFailure testCollectionCleanupFailure)
        {
            // TODO: Add a collection node?
            var methodMessage = string.Format("Collection cleanup failed in {0}",
                testCollectionCleanupFailure.TestCollection.DisplayName);
            HandleCleanupFailure(testCollectionCleanupFailure, testCollectionCleanupFailure.TestCases, methodMessage);
            return server.ShouldContinue;
        }

        protected override bool Visit(ITestClassCleanupFailure testClassCleanupFailure)
        {
            var methodMessage = string.Format("Class cleanup failed in {0}", testClassCleanupFailure.TestClass.Class.Name);
            HandleCleanupFailure(testClassCleanupFailure, testClassCleanupFailure.TestCases, methodMessage);
            return server.ShouldContinue;
        }

        private void HandleCleanupFailure(IFailureInformation failureInformation, IEnumerable<ITestCase> testCases, string methodMessage)
        {
            var testClasses = from testCase in testCases
                select testCase.TestMethod.TestClass.Class.Name;

            string classMessage;
            var methodExceptions = new[] { new TaskException(null, methodMessage, null) };
            var classExceptions = ConvertExceptions(failureInformation, out classMessage);

            foreach (var typeName in testClasses.Distinct())
            {
                var taskInfo = taskProvider.GetClassTask(typeName);

                // TODO: Should fail all test cases rather than have to find descendants
                foreach (var task in taskProvider.GetDescendants(taskInfo))
                {
                    server.TaskException(task.RemoteTask, methodExceptions);
                    server.TaskFinished(task.RemoteTask, methodMessage, TaskResult.Error, 0);
                }

                server.TaskException(taskInfo.RemoteTask, classExceptions);

                taskInfo.Message = classMessage;
                taskInfo.Result = TaskResult.Exception;

                if (taskInfo.Finished)
                {
                    taskInfo.Finished = false;
                    TaskFinished(taskInfo, taskInfo.Message, TaskResult.Exception, 0);
                }
            }

        }

        protected override bool Visit(ITestMethodCleanupFailure testMethodCleanupFailure)
        {
            Console.WriteLine("ITestMethodCleanupFailure");

            // TODO: What causes this? Need to create a test

#if false

            var taskInfo = taskProvider.GetMethodTask(testMethodCleanupFailure.TestClass.Class.Name,
                testMethodCleanupFailure.TestMethod.Method.Name);

            string message;
            var exceptions = ConvertExceptions(testMethodCleanupFailure, out message);

            taskInfo.Result = TaskResult.Exception;
            taskInfo.Message = message;

            server.TaskException(taskInfo.RemoteTask, exceptions);

#endif

            return server.ShouldContinue;
        }

        protected override bool Visit(ITestCaseCleanupFailure testCaseCleanupFailure)
        {
            Console.WriteLine("ITestCaseCleanupFailure");

#if false
            // TODO: Perhaps look for theory task?
            var taskInfo = taskProvider.GetMethodTask(testCaseCleanupFailure.TestClass.Class.Name,
                testCaseCleanupFailure.TestMethod.Method.Name);

            string message;
            var exceptions = ConvertExceptions(testCaseCleanupFailure, out message);

            taskInfo.Result = TaskResult.Exception;
            taskInfo.Message = message;

            server.TaskException(taskInfo.RemoteTask, exceptions);
#endif

            return server.ShouldContinue;
        }

        protected override bool Visit(ITestCleanupFailure testCleanupFailure)
        {
            Console.WriteLine("ITestCleanupFailure");

#if false
            // TODO: Perhaps look for theory task?
            var taskInfo = taskProvider.GetMethodTask(testCleanupFailure.TestClass.Class.Name,
                testCleanupFailure.TestMethod.Method.Name);

            string message;
            var exceptions = ConvertExceptions(testCleanupFailure, out message);

            taskInfo.Result = TaskResult.Exception;
            taskInfo.Message = message;

            server.TaskException(taskInfo.RemoteTask, exceptions);
#endif

            return server.ShouldContinue;
        }

        protected override bool Visit(ITestClassFinished testClassFinished)
        {
            // TODO: Report a class as skipped if any (all?) skipped?
            var result = testClassFinished.TestsFailed > 0 ? TaskResult.Exception : TaskResult.Success;
            var message = result == TaskResult.Exception ? OneOrMoreChildTestsFailedMessage : string.Empty;

            var classTaskInfo = taskProvider.GetClassTask(testClassFinished.TestClass.Class.Name);
            TaskFinished(classTaskInfo, message, result, testClassFinished.ExecutionTime);

            return server.ShouldContinue;
        }

        protected override bool Visit(ITestMethodStarting testMethodStarting)
        {
            var taskInfo = taskProvider.GetMethodTask(testMethodStarting.TestClass.Class.Name, testMethodStarting.TestMethod.Method.Name);
            TaskStarting(taskInfo);

            taskInfo.Finished = false;

            taskInfo.Start();
            return server.ShouldContinue;
        }

        protected override bool Visit(ITestOutput testCaseOutput)
        {
            var task = GetCurrentTaskInfo(testCaseOutput);
            TaskOutput(task, testCaseOutput);
            return server.ShouldContinue;
        }

        protected override bool Visit(ITestSkipped testSkipped)
        {
            var task = GetCurrentTaskInfo(testSkipped);
            TaskFinished(task, testSkipped.Reason, TaskResult.Skipped, testSkipped.ExecutionTime);
            return server.ShouldContinue;
        }

        // Called when a test is starting its run. Called for each test command, not necessarily
        // each test method (i.e. once for each theory data row)
        protected override bool Visit(ITestStarting testStarting)
        {
            // We've already started the method, so only start for a theory
            var theoryTask = GetNextTheoryTaskInfo(testStarting);
            if (theoryTask != null)
                TaskStarting(theoryTask);
            return server.ShouldContinue;
        }

        protected override bool Visit(ITestPassed testPassed)
        {
            var task = GetCurrentTaskInfo(testPassed);
            TaskFinished(task, string.Empty, TaskResult.Success, testPassed.ExecutionTime);
            return server.ShouldContinue;
        }

        protected override bool Visit(ITestFailed testFailed)
        {
            var taskInfo = GetCurrentTaskInfo(testFailed);

            string message;
            var exceptions = ConvertExceptions(testFailed, out message);
            server.TaskException(taskInfo.RemoteTask, exceptions);

            TaskFinished(taskInfo, message, TaskResult.Exception, testFailed.ExecutionTime);

            return server.ShouldContinue;
        }

        protected override bool Visit(ITestMethodFinished testMethodFinished)
        {
            var methodTask = taskProvider.GetMethodTask(testMethodFinished.TestClass.Class.Name, testMethodFinished.TestMethod.Method.Name);
            if (!methodTask.Finished)
            {
                // TODO: Report method as skipped if all theories are skipped?
                var result = taskProvider.GetTheories(methodTask).Aggregate(TaskResult.Success,
                    (a, tti) => tti.Result == TaskResult.Error || tti.Result == TaskResult.Exception ? tti.Result : a);

                var message = string.Empty;
                if (result != TaskResult.Success)
                    message = OneOrMoreChildTestsFailedMessage;

                // TODO: Stopwatch? Really?
                TaskFinished(methodTask, message, result, (decimal) methodTask.Stop().TotalSeconds);
            }

            return server.ShouldContinue;
        }

        private TaskInfo GetCurrentTaskInfo(ITestMessage test)
        {
            var theoryTask = GetCurrentTheoryTaskInfo(test);
            if (theoryTask != null)
                return theoryTask;

            var methodTask = taskProvider.GetMethodTask(test.TestClass.Class.Name, test.TestMethod.Method.Name);
            return methodTask;
        }

        // These are thread safe, because our smallest unit of concurrency is class
        private TheoryTaskInfo GetCurrentTheoryTaskInfo(ITestMessage test)
        {
            return GetTheoryTaskInfo(test, theoryTask => theoryTask.Started && !theoryTask.Finished);
        }

        private TheoryTaskInfo GetNextTheoryTaskInfo(ITestMessage test)
        {
            return GetTheoryTaskInfo(test, theoryTask => !theoryTask.Started);
        }

        private TheoryTaskInfo GetTheoryTaskInfo(ITestMessage test, Func<TheoryTaskInfo, bool> predicate)
        {
            var displayName = test.Test.DisplayName;
            var className = test.TestClass.Class.Name;
            var methodName = test.TestMethod.Method.Name;

            // TODO: Use test.TestCase.UniqueID here
            var theoryTask = taskProvider.GetTheoryTask(displayName, className, methodName);
            for (var i = 2; theoryTask != null && !predicate(theoryTask); i++)
            {
                displayName = string.Format("{0} [{1}]", test.Test.DisplayName, i);
                theoryTask = taskProvider.GetTheoryTask(displayName, className, methodName);
            }
            return theoryTask;
        }

        private void TaskOutput(TaskInfo task, ITestOutput output)
        {
            if (!string.IsNullOrEmpty(output.Output))
                server.TaskOutput(task.RemoteTask, output.Output, TaskOutputType.STDOUT);
        }

        private void TaskStarting(TaskInfo taskInfo)
        {
            server.TaskStarting(taskInfo.RemoteTask);
            taskInfo.Started = true;
        }

        private void TaskFinished(TaskInfo taskInfo, string message, TaskResult taskResult, decimal executionTime)
        {
            if (taskInfo.Finished)
                return;

            if (taskInfo.Result != TaskResult.Inconclusive)
                taskResult = taskInfo.Result;
            if (!string.IsNullOrEmpty(taskInfo.Message))
                message = taskInfo.Message;

            server.TaskFinished(taskInfo.RemoteTask, message, taskResult, executionTime);
            taskInfo.Finished = true;
            taskInfo.Result = taskResult;
        }

        private TaskException[] ConvertExceptions(IFailureInformation failure, out string simplifiedMessage)
        {
            var exceptions = new List<TaskException>();

            for (var i = 0; i < failure.ExceptionTypes.Length; i++)
            {
                // Strip out the xunit assert methods from the stack traces by taking
                // out anything in the Xunit.Assert namespace
                var stackTraces = failure.StackTraces[i] != null ? failure.StackTraces[i]
                    .Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                    .Where(s => !s.Contains("Xunit.Assert")).Join(Environment.NewLine) : string.Empty;

                exceptions.Add(new TaskException(failure.ExceptionTypes[i], failure.Messages[i], stackTraces));
            }

            // Simplified message - if it's an xunit native exception (most likely an assert)
            // only include the exception message, otherwise include the exception type
            var exceptionMessage = failure.Messages[0];
            var exceptionType = failure.ExceptionTypes[0];
            exceptionType = exceptionType.StartsWith("Xunit") ? string.Empty : (exceptionType + ": ");
            simplifiedMessage = exceptionMessage.StartsWith(failure.ExceptionTypes[0]) ? exceptionMessage : exceptionType + exceptionMessage;

            return exceptions.ToArray();
        }
    }
}
