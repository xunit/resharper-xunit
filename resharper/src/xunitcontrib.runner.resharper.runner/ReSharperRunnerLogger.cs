using System;
using System.Collections.Generic;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public class ReSharperRunnerLogger : IRunnerLogger
    {
        private readonly RemoteTaskServer server;
        private readonly TaskProvider taskProvider;
        private readonly Stack<TaskState> states = new Stack<TaskState>();
        private readonly HashSet<RemoteTask> runTests = new HashSet<RemoteTask>(); 

        private class TaskState
        {
            public TaskState(RemoteTask task, TaskState parentState, string message = "Internal Error (xunit runner): No status reported", TaskResult result = TaskResult.Inconclusive)
            {
                Task = task;
                Message = message;
                Result = result;
                ParentState = parentState;
            }

            public readonly RemoteTask Task;
            public TaskResult Result;
            public readonly TaskState ParentState;
            public string Message;
            public TimeSpan Duration;

            public void SetPassed()
            {
                Result = TaskResult.Success;
                Message = string.Empty;
            }

            public bool IsTheory()
            {
                return Task is XunitTestTheoryTask;
            }
        }

        public ReSharperRunnerLogger(RemoteTaskServer server, TaskProvider taskProvider)
        {
            this.server = server;
            this.taskProvider = taskProvider;
        }

        private TaskState CurrentState { get { return states.Peek(); } }

        public void AssemblyStart(string assemblyFilename, string configFilename, string xUnitVersion)
        {
            // This won't get called - we tell xunit to run a long list of methods, not an entire
            // assembly. It might very well be all the methods in an assembly, but we need to be able
            // to pick and choose which gets run and which are not
        }

        public void AssemblyFinished(string assemblyFilename, int total, int failed, int skipped, double time)
        {
            // See AssemblyStart
        }

        // Not part of xunit's API, but convenient to place here
        public void ClassStart(string type)
        {
            states.Push(new TaskState(taskProvider.GetClassTask(type), null, string.Empty, TaskResult.Success));
            server.TaskStarting(CurrentState.Task);
        }

        // Called when a class failure is encountered (i.e., when a fixture from IUseFixture throws an 
        // exception during construction or System.IDisposable.Dispose)
        // If the exception happens in the class (fixture) construtor, the child tests are not run, so
        // we need to mark them all as having failed
        public bool ClassFailed(string className, string exceptionType, string message, string stackTrace)
        {
            var methodMessage = string.Format("Class failed in {0}", className);

            // Make sure all of the child methods are marked as failures. If not, it's very easy to miss
            // exceptions - you have a bunch of passing tests, but the parent node is marked as a failure
            // (and not counted in the failing tests count). Failing all tests makes this apparent
            foreach (var task in taskProvider.GetDescendants(className))
            {
                server.TaskException(task, new[] { new TaskException(null, methodMessage, null) } ); 
                server.TaskFinished(task, methodMessage, TaskResult.Error);
            }

            var state = CurrentState;
            state.Result = TaskResult.Exception;

            server.TaskException(state.Task, ExceptionConverter.ConvertExceptions(exceptionType, message, stackTrace, out state.Message));

            // Let's carry on running tests - I guess if it were a catastrophic error, we could chose to abort
            return server.ShouldContinue;
        }

        // Not part of xunit's API, but convenient to place here. When this is called, ClassFailed
        // might already have been called, in which case it will already have called TaskFinished
        public void ClassFinished()
        {
            // The classResult should not be a reflection of what tests passed, failed or were skipped,
            // but should indicate if the class executed its tests correctly

            while(states.Count > 0)
            {
                var state = states.Pop();
                server.TaskFinished(state.Task, state.Message, state.Result, state.Duration);
            }
        }

        // After ExceptionThrown is called, no other methods are called (unless we are running an assembly,
        // and then we get notified about xml transformations)
        public void ExceptionThrown(string assemblyFilename, Exception exception)
        {
            // The nunit runner uses TE.ConverExceptions whenever an exception occurs in infrastructure
            // code. When a test fails (and there's always an exception for a failing test, it builds the
            // TaskExceptions itself). ConvertExceptions uses the short name of the exception type, and
            // the nunit runner uses the full type. We're following their lead, but almost by coincidence.
            // We're using the JetBrains function here, and can't use it for a failing test, because we
            // get a text version of the thrown exceptions, rather than an actual Exception we could pass
            // into this method.
            // In other words - if a class fails due to a dodgy exception, the message has a short type name.
            // If a test fails due to an exception, the message has a fully qualified type name
            var state = CurrentState;
            server.TaskException(state.Task, TaskExecutor.ConvertExceptions(exception, out state.Message));
            state.Result = TaskResult.Exception;
        }

        // Called instead of TestStart, TestFinished is still called
        public void TestSkipped(string name, string type, string method, string reason)
        {
            var state = CurrentState;
            if (!state.IsTheory())
            {
                var task = taskProvider.GetMethodTask(name, type, method);

                // SkipCommand returns null from ToStartXml, so we don't get notified via TestStart. Custom
                // commands might not do this, so we make sure we don't notify task starting twice
                StartNewMethodTaskIfNotAlreadyRunning(task);
            }

            CurrentState.Result = TaskResult.Skipped;
            CurrentState.Message = reason;
        }

        // Called when a test is starting its run. Called for each test command, not necessarily
        // each test method (i.e. once for each theory data row)
        public bool TestStart(string name, string type, string method)
        {
            var methodTask = taskProvider.GetMethodTask(name, type, method);

            FinishPreviousMethodTaskIfStillRunning(methodTask);
            StartNewMethodTaskIfNotAlreadyRunning(methodTask);
            StartNewTheoryTaskIfRequired(name, type, method);

            return server.ShouldContinue;
        }

        private void FinishPreviousMethodTaskIfStillRunning(RemoteTask methodTask)
        {
            // Can still be running if we ran a theory, because TestFinish will only pop the theory,
            // not the owning method
            if (!Equals(methodTask, CurrentState.Task) && CurrentState.Task is XunitTestMethodTask)
                TestFinished(null, null, null);
        }

        private void StartNewMethodTaskIfNotAlreadyRunning(RemoteTask methodTask)
        {
            if (!Equals(methodTask, CurrentState.Task))
            {
                states.Push(new TaskState(methodTask, CurrentState));
                server.TaskStarting(methodTask);
            }
        }

        private void StartNewTheoryTaskIfRequired(string name, string type, string method)
        {
            var theoryTask = GetTheoryTask(name, type, method);
            if (theoryTask != null)
            {
                runTests.Add(theoryTask);

                // The current state is the parent method task. Mark it as having run, or
                // we'll report it as Inconclusive
                CurrentState.SetPassed();

                states.Push(new TaskState(theoryTask, CurrentState));
                server.TaskStarting(theoryTask);
            }
        }

        private RemoteTask GetTheoryTask(string name, string type, string method)
        {
            var task = taskProvider.GetTheoryTask(name, type, method);
            if (task != null)
            {
                var i = 1;
                while (runTests.Contains(task))
                {
                    var newName = string.Format("{1} [{0}]", ++i, name);
                    task = taskProvider.GetTheoryTask(newName, type, method);
                }
            }
            return task;
        }

        public void TestPassed(string name, string type, string method, double duration, string output)
        {
            var state = CurrentState;

            // We can only assume that it's stdout
            if (!string.IsNullOrEmpty(output))
                server.TaskOutput(state.Task, output, TaskOutputType.STDOUT);

            state.SetPassed();
            state.Duration = TimeSpan.FromSeconds(duration);
        }

        public void TestFailed(string name, string type, string method, double duration, string output, string exceptionType, string message, string stackTrace)
        {
            var state = CurrentState;

            if (state.ParentState != null)
            {
                state.ParentState.Result = TaskResult.Exception;
                state.ParentState.Message = "One or more child tests failed";
            }

            // We can only assume that it's stdout
            if (!string.IsNullOrEmpty(output))
                server.TaskOutput(state.Task, output, TaskOutputType.STDOUT);

            state.Duration = TimeSpan.FromSeconds(duration);

            state.Result = TaskResult.Exception;
            server.TaskException(state.Task, ExceptionConverter.ConvertExceptions(exceptionType, message, stackTrace, out state.Message));
        }

        // This gets called after TestPassed, TestFailed or TestSkipped
        public bool TestFinished(string name, string type, string method)
        {
            var state = states.Pop();
            server.TaskFinished(state.Task, state.Message, state.Result, state.Duration);

            return server.ShouldContinue;
        }
    }
}
