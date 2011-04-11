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
        private IList<XunitTestMethodTask> methodTasks;

        private TaskResult classResult;
        private string classFinishMessage = string.Empty;

        private TaskResult testResult;
        private string testFinishMessage;
        private string currentMethod;

        public ReSharperRunnerLogger(IRemoteTaskServer server, XunitTestClassTask classTask)
        {
            this.server = server;
            this.classTask = classTask;
        }

        public IList<XunitTestMethodTask> MethodTasks
        {
            set { methodTasks = value; }
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
            server.TaskException(classTask, ExceptionConverter.ConvertExceptions(exceptionType, message, stackTrace, out classFinishMessage));
            classResult = TaskResult.Exception;

            // Let's carry on running tests - I guess if it were a catastrophic error, we could chose to abort
            return true;
        }

        // Not part of xunit's API, but convenient to place here. When this is called, ClassFailed
        // might already have been called, in which case it will already have called TaskFinished
        public void ClassFinished()
        {
            EnsureCurrentTestMethodFinished(null);
            currentMethod = null;

            // The classResult should not be a reflection of what tests passed, failed or were skipped,
            // but should indicate if the class executed its tests correctly
            server.TaskFinished(classTask, classFinishMessage, classResult);
        }

        // After ExceptionThrown is called, no other methods are called (unless we are running an assembly,
        // and then we get notified about xml transformations)
        public void ExceptionThrown(string assemblyFilename, Exception exception)
        {
            server.TaskException(classTask, TaskExecutor.ConvertExceptions(exception, out classFinishMessage));
            classResult = TaskResult.Exception;
        }

        private RemoteTask GetMethodTask(string methodName)
        {
            // We'd need a lot of methods in a test for this to become unacceptable, performance-wise
            foreach (XunitTestMethodTask task in methodTasks)
            {
                if (task.MethodName == methodName)
                    return task;
            }

            return null;
        }

        private void EnsureCurrentTestMethodFinished(string method)
        {
            if (currentMethod != method && currentMethod != null)
                server.TaskFinished(GetMethodTask(currentMethod), testFinishMessage, testResult);
        }

        // Called instead of TestStart/TestFinished
        public void TestSkipped(string name, string type, string method, string reason)
        {
            EnsureCurrentTestMethodFinished(method);
            currentMethod = method;

            server.TaskStarting(GetMethodTask(method));

            server.TaskExplain(GetMethodTask(method), reason);
            testResult = TaskResult.Skipped;
            testFinishMessage = reason;
        }

        // Called when a test is starting its run. Called for each test command, not necessarily
        // each test method (i.e. once for each theory data row)
        public bool TestStart(string name, string type, string method)
        {
            EnsureCurrentTestMethodFinished(method);
            if (currentMethod != method)
            {
                testResult = TaskResult.Success;
                testFinishMessage = string.Empty;
                server.TaskStarting(GetMethodTask(method));
                currentMethod = method;
            }
            return true;
        }

        public void TestPassed(string name, string type, string method, double duration, string output)
        {
            // We can only assume that it's stdout
            if (!string.IsNullOrEmpty(output))
                server.TaskOutput(GetMethodTask(method), output, TaskOutputType.STDOUT);

            // Do nothing - we've already set up the defaults for success, and don't overwrite an error
        }

        public void TestFailed(string name, string type, string method, double duration, string output, string exceptionType, string message, string stackTrace)
        {
            RemoteTask task = GetMethodTask(method);

            // We can only assume that it's stdout
            if (!string.IsNullOrEmpty(output))
                server.TaskOutput(task, output, TaskOutputType.STDOUT);

            testResult = TaskResult.Exception;
            server.TaskException(task, ExceptionConverter.ConvertExceptions(exceptionType, message, stackTrace, out testFinishMessage));
        }

        // This gets called after TestPassed, TestFailed or TestSkipped
        public bool TestFinished(string name, string type, string method)
        {
            // Return true if we want to continue running the tests
            return true;
        }
    }
}
