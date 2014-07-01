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

        private readonly Stack<TaskInfo> nastySharedState = new Stack<TaskInfo>();

        private void NastySharedState_PushCurrent(TaskInfo task)
        {
            nastySharedState.Push(task);
        }

        private void NastySharedState_PopCurrent(TaskInfo expectedTaskInfo)
        {
            var poppedState = nastySharedState.Pop();
            if (!Equals(poppedState.RemoteTask, expectedTaskInfo.RemoteTask))
            {
                server.TaskFinished(expectedTaskInfo.RemoteTask, "Internal (xunit runner): Shared state out of sync!?", TaskResult.Error,
                    TimeSpan.Zero);
            }
        }

        protected override bool Visit(ITestClassStarting testClassStarting)
        {
            var classTaskInfo = taskProvider.GetClassTask(testClassStarting.TestClass.Class.Name);
            TaskStarting(classTaskInfo);
            return server.ShouldContinue;
        }

        // Called when a class failure is encountered (i.e., when a fixture from IUseFixture throws an 
        // exception during construction or System.IDisposable.Dispose)
        // If the exception happens in the class (fixture) construtor, the child tests are not run, so
        // we need to mark them all as having failed
        // xunit2: Seems to be when a class or collection fixture fails

        // Called for catastrophic errors, e.g. ambiguously named methods in xunit1
        protected override bool Visit(IErrorMessage error)
        {
            // TODO: This is very nasty
            // OK. I know this will only get called in these scenarios:
            // 1. Collection fixtures Dispose methods throw exceptions
            //    Potentially called multiple times, class and methods are still run
            // 2. Class fixtures Dispose methods throw exceptions
            //    Potentially called multiple times, methods are still run
            // 3. xunit1 class fixture ctor or dispose 
            //    Called once, per class. Methods not run
            // 4. xunit1 catastrophic error, e.g. ambiguous methods
            //    Class and methods not run
            var taskInfo = nastySharedState.Peek();

            string message;
            var exceptions = ConvertExceptions(error, out message);

            if (taskInfo.RemoteTask is XunitTestClassTask)
            {
                var classTaskInfo = (ClassTaskInfo) taskInfo;
                var methodMessage = string.Format("Class failed in {0}", classTaskInfo.ClassTask.TypeName);

                var methodExceptions = new[] {new TaskException(null, methodMessage, null)};
                foreach (var task in taskProvider.GetDescendants(classTaskInfo))
                {
                    server.TaskException(task.RemoteTask, methodExceptions);
                    server.TaskFinished(task.RemoteTask, methodMessage, TaskResult.Error, 0);
                }
            }

            taskInfo.Result = TaskResult.Exception;
            taskInfo.Message = message;

            server.TaskException(taskInfo.RemoteTask, exceptions);

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

            // BUG: beta1+2 will report overloaded methods starting/finished twice
            // Since the method is only keyed on name, it s I'll submit a PR to fix
            taskInfo.Finished = false;

            taskInfo.Start();
            return server.ShouldContinue;
        }

        protected override bool Visit(ITestSkipped testSkipped)
        {
            var task = GetCurrentTaskInfo(testSkipped);
            TaskOutput(task, testSkipped);
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
            TaskOutput(task, testPassed);
            TaskFinished(task, string.Empty, TaskResult.Success, testPassed.ExecutionTime);
            return server.ShouldContinue;
        }

        protected override bool Visit(ITestFailed testFailed)
        {
            var taskInfo = GetCurrentTaskInfo(testFailed);

            TaskOutput(taskInfo, testFailed);

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
            var displayName = test.TestDisplayName;
            var className = test.TestClass.Class.Name;
            var methodName = test.TestMethod.Method.Name;

            // TODO: Use test.TestCase.UniqueID here
            var theoryTask = taskProvider.GetTheoryTask(displayName, className, methodName);
            for (var i = 2; theoryTask != null && !predicate(theoryTask); i++)
            {
                displayName = string.Format("{0} [{1}]", test.TestDisplayName, i);
                theoryTask = taskProvider.GetTheoryTask(displayName, className, methodName);
            }
            return theoryTask;
        }

        private void TaskOutput(TaskInfo task, ITestResultMessage result)
        {
            if (!string.IsNullOrEmpty(result.Output))
                server.TaskOutput(task.RemoteTask, result.Output, TaskOutputType.STDOUT);
        }

        private void TaskStarting(TaskInfo taskInfo)
        {
            server.TaskStarting(taskInfo.RemoteTask);
            taskInfo.Started = true;

            NastySharedState_PushCurrent(taskInfo);
        }

        private void TaskFinished(TaskInfo taskInfo, string message, TaskResult taskResult, decimal executionTime)
        {
            NastySharedState_PopCurrent(taskInfo);

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
                var stackTraces = failure.StackTraces[i]
                    .Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                    .Where(s => !s.Contains("Xunit.Assert")).Join(Environment.NewLine);

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
