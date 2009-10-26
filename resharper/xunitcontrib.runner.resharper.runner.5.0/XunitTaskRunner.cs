using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public class XunitTaskRunner : RecursiveRemoteTaskRunner
    {
        public const string RunnerId = "xUnit";

        private ExecutorWrapper executorWrapper;
        private string priorCurrentDirectory;

        public XunitTaskRunner(IRemoteTaskServer server) : base(server)
        {
        }

        // Called to prepare this task. Shouldn't throw
        // Server.TaskStarting is called before this
        // If Start returns TaskError or TaskException, Server.TaskFinished is called
        // If Start returns TaskSkipped, all child nodes have server.TaskFinished(TaskResult.Skipped)
        // called recursively. It is up to Start to call TaskFinished(Skipped) for the current node
        public override TaskResult Start(TaskExecutionNode node)
        {
            XunitTestAssemblyTask assemblyTask = (XunitTestAssemblyTask) node.RemoteTask;

            // Set the current directory to the assembly location. This is something that ReSharper's
            // AssemblyLoadTask would do for us, but since we're not using it, we need to do it
            priorCurrentDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = Path.GetDirectoryName(assemblyTask.AssemblyLocation);

            executorWrapper = new ExecutorWrapper(assemblyTask.AssemblyLocation, null, true);

            return TaskResult.Success;
        }

        // Called to run the task, unless we implement RecursiveRemoteTaskRunner, in which case it won't
        // Can throw. Exception will be caught and logged with Server.TaskException
        // Finish will be called, and then Server.TaskFinished - but with TaskResult.Exception
        // Return TaskSuccess to get the child nodes executed
        public override TaskResult Execute(TaskExecutionNode node)
        {
            // This will not get called, because we derive from RecursiveRemoteTaskRunner
            return TaskResult.Error;
        }

        // Called to handle all the nodes ourselves
        public override void ExecuteRecursive(TaskExecutionNode node)
        {
            foreach (TaskExecutionNode childNode in node.Children)
            {
                XunitTestClassTask classTask = (XunitTestClassTask) childNode.RemoteTask;

                ReSharperRunnerLogger runnerLogger = new ReSharperRunnerLogger(Server, classTask);
                runnerLogger.ClassStart();
                Server.TaskStarting(classTask);

                // TODO: try/catch or at least try/finally?
                IList<XunitTestMethodTask> methodTasks = new List<XunitTestMethodTask>();
                List<string> methodNames = new List<string>();
                foreach (TaskExecutionNode methodNode in childNode.Children)
                {
                    XunitTestMethodTask methodTask = (XunitTestMethodTask) methodNode.RemoteTask;
                    methodTasks.Add(methodTask);
                    methodNames.Add(methodTask.MethodName);
                }
                runnerLogger.MethodTasks = methodTasks;

                TestRunner runner = new TestRunner(executorWrapper, runnerLogger);
                // Don't capture the result of the test run - ReSharper gathers that as we go
                runner.RunTests(classTask.TypeName, methodNames);

                runnerLogger.ClassFinished();
            }
        }

        // Called for cleanup purposes. For a successful run, the return value of this is
        // the return value of the task. If an exception is thrown, the return value is
        // ignored and TaskException is returned. If the node is skipped, this doesn't get
        // called.
        // Should not throw
        public override TaskResult Finish(TaskExecutionNode node)
        {
            // I think this leaks if we skip at the assembly level, but I don't think that's
            // possible. Also, the process goes away after this call, so we get cleaned up
            // then. I think we're ok.
            executorWrapper.Dispose();

            Environment.CurrentDirectory = priorCurrentDirectory;

            return TaskResult.Success;
        }
    }
}
