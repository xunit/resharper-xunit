using System;
using System.Collections.Generic;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public class ReSharperRunnerLogger : IRunnerLogger
    {
        private readonly IRemoteTaskServer server;
        private readonly ITaskProvider taskProvider;
        private readonly Stack<TaskState> states = new Stack<TaskState>();

        private class TaskState
        {
            public TaskState(RemoteTask task, string message = null, TaskResult result = TaskResult.Success)
            {
                Task = task;
                Message = message;
                Result = result;
            }

            public readonly RemoteTask Task;
            public TaskResult Result;
            public string Message;
        }

        public ReSharperRunnerLogger(IRemoteTaskServer server, XunitTestClassTask classTask, ITaskProvider taskProvider)
        {
            this.server = server;
            this.taskProvider = taskProvider;

            states.Push(new TaskState(classTask));
        }

        private TaskState CurrentState { get { return states.Peek(); } }

        public void AssemblyStart(string assemblyFilename, string configFilename, string xUnitVersion)
        {
            // This won't get called - we tell xunit to run a long list of methods, not an entire
            // assembly. It might very well be all the methods in an assembly, but we need to be able
            // to pick and choose which gets run and which are not

            // TODO: Do we need to do anything to handle configFilename?
        }

        public void AssemblyFinished(string assemblyFilename, int total, int failed, int skipped, double time)
        {
            // See AssemblyStart
        }

        // Not part of xunit's API, but convenient to place here
        public void ClassStart()
        {
            server.TaskStarting(CurrentState.Task);
        }

        // Called when a class failure is encountered (i.e., when a fixture from IUseFixture throws an 
        // exception during construction or System.IDisposable.Dispose)
        // Called after all tests within the class have been started and finished
        public bool ClassFailed(string className, string exceptionType, string message, string stackTrace)
        {
            var methodMessage = string.Format("Class failed in {0}", className);

            // TODO: I'd like to get rid of taskProvider.MethodTasks
            foreach (var methodTask in taskProvider.MethodTasks)
            {
                server.TaskException(methodTask, new[] { new TaskException(null, methodMessage, null) } ); 
                server.TaskFinished(methodTask, methodMessage, TaskResult.Error);
            }

            var state = CurrentState;
            state.Result = TaskResult.Exception;

            server.TaskException(state.Task, ExceptionConverter.ConvertExceptions(exceptionType, message, stackTrace, out state.Message));

            // Let's carry on running tests - I guess if it were a catastrophic error, we could chose to abort
            return true;
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
                server.TaskFinished(state.Task, state.Message, state.Result);
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
            var task = taskProvider.GetTask(name, type, method);
            server.TaskStarting(task);
            server.TaskExplain(task, reason);

            var state = new TaskState(task, reason, TaskResult.Skipped);
            states.Push(state);
        }

        // Called when a test is starting its run. Called for each test command, not necessarily
        // each test method (i.e. once for each theory data row)
        public bool TestStart(string name, string type, string method)
        {
            var state = CurrentState;
            var task = taskProvider.GetTask(name, type, method);

            var theoryTask = task as XunitTestTheoryTask;
            if (theoryTask != null)
            {
                var methodTask = taskProvider.GetTask(theoryTask.ParentElementId);
                if (methodTask != state.Task)
                {
                    if (state.Task is XunitTestMethodTask)
                    {
                        state = states.Pop();
                        server.TaskFinished(state.Task, state.Message, state.Result);
                    }

                    states.Push(new TaskState(methodTask));
                    server.TaskStarting(methodTask);
                }
            }

            states.Push(new TaskState(task));
            server.TaskStarting(task);

            return true;
        }

        public void TestPassed(string name, string type, string method, double duration, string output)
        {
            // We can only assume that it's stdout
            if (!string.IsNullOrEmpty(output))
                server.TaskOutput(taskProvider.GetTask(name, type, method), output, TaskOutputType.STDOUT);

            // Do nothing - we've already set up the defaults for success, and don't overwrite an error
        }

        public void TestFailed(string name, string type, string method, double duration, string output, string exceptionType, string message, string stackTrace)
        {
            var state = CurrentState;

            // We can only assume that it's stdout
            if (!string.IsNullOrEmpty(output))
                server.TaskOutput(state.Task, output, TaskOutputType.STDOUT);

            state.Result = TaskResult.Exception;
            server.TaskException(state.Task, ExceptionConverter.ConvertExceptions(exceptionType, message, stackTrace, out state.Message));
        }

        // This gets called after TestPassed, TestFailed or TestSkipped
        public bool TestFinished(string name, string type, string method)
        {
            var state = states.Pop();
            server.TaskFinished(state.Task, state.Message, state.Result);

            // Return true if we want to continue running the tests
            return true;
        }
    }
}
