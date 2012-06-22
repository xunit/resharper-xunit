using System;
using System.Collections.Generic;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public class ReSharperRunnerLogger : IRunnerLogger
    {
        private readonly IRemoteTaskServer server;
        private readonly XunitTestClassTask classTask;
        private readonly ITaskProvider taskProvider;

        private TaskResult classResult;
        private string classFinishMessage = string.Empty;

        private TaskResult methodResult;
        private string testFinishMessage;

        public ReSharperRunnerLogger(IRemoteTaskServer server, XunitTestClassTask classTask, ITaskProvider taskProvider)
        {
            this.server = server;
            this.classTask = classTask;
            this.taskProvider = taskProvider;
        }

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
            server.TaskStarting(classTask);
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

            server.TaskException(classTask, ExceptionConverter.ConvertExceptions(exceptionType, message, stackTrace, out classFinishMessage));
            classResult = TaskResult.Exception;

            // Let's carry on running tests - I guess if it were a catastrophic error, we could chose to abort
            return true;
        }

        // Not part of xunit's API, but convenient to place here. When this is called, ClassFailed
        // might already have been called, in which case it will already have called TaskFinished
        public void ClassFinished()
        {
            if (currentMethodTask != null)
                server.TaskFinished(currentMethodTask, string.Empty, TaskResult.Success);

            // The classResult should not be a reflection of what tests passed, failed or were skipped,
            // but should indicate if the class executed its tests correctly
            server.TaskFinished(classTask, classFinishMessage, classResult);
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
            server.TaskException(classTask, TaskExecutor.ConvertExceptions(exception, out classFinishMessage));
            classResult = TaskResult.Exception;
        }

        // Called instead of TestStart/TestFinished
        public void TestSkipped(string name, string type, string method, string reason)
        {
            var task = taskProvider.GetTask(name, type, method);
            server.TaskStarting(task);
            server.TaskExplain(task, reason);

            testFinishMessage = reason;
            methodResult = TaskResult.Skipped;
        }

        private XunitTestMethodTask currentMethodTask;

        // Called when a test is starting its run. Called for each test command, not necessarily
        // each test method (i.e. once for each theory data row)
        public bool TestStart(string name, string type, string method)
        {
            var task = taskProvider.GetTask(name, type, method);
            if (taskProvider.IsTheory(name, type, method))
            {
                var theoryTask = (XunitTestTheoryTask) task;
                if (currentMethodTask == null || theoryTask.ParentId != currentMethodTask.Id)
                {
                    if (currentMethodTask != null)
                        server.TaskFinished(currentMethodTask, string.Empty, TaskResult.Success);
                    var parentTask = (XunitTestMethodTask)taskProvider.GetTask(theoryTask.ParentId);
                    server.TaskStarting(parentTask);
                    currentMethodTask = parentTask;
                }
            }

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
            var task = taskProvider.GetTask(name, type, method);

            // We can only assume that it's stdout
            if (!string.IsNullOrEmpty(output))
                server.TaskOutput(task, output, TaskOutputType.STDOUT);

            methodResult = TaskResult.Exception;
            server.TaskException(task, ExceptionConverter.ConvertExceptions(exceptionType, message, stackTrace, out testFinishMessage));
        }

        // This gets called after TestPassed, TestFailed or TestSkipped
        public bool TestFinished(string name, string type, string method)
        {
            server.TaskFinished(taskProvider.GetTask(name, type, method), testFinishMessage, methodResult);

            // Return true if we want to continue running the tests
            return true;
        }
    }
}
