using System;
using System.IO;
using System.Linq;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public class XunitTestRunner : RecursiveRemoteTaskRunner
    {
        public const string RunnerId = "xUnit";

        private ExecutorWrapper executorWrapper;
        private string priorCurrentDirectory;

        public XunitTestRunner(IRemoteTaskServer server) : base(server)
        {
        }

        // Called to prepare this task. Shouldn't throw
        // Server.TaskStarting is called before this
        // If Start returns TaskError or TaskException, Server.TaskFinished is called
        // If Start returns TaskSkipped, all child nodes have server.TaskFinished(TaskResult.Skipped)
        // called recursively. It is up to Start to call TaskFinished(Skipped) for the current node
        //
        // This method is called from the default app domain and is handed a node that is the root
        // of the tree of actions to perform (the tree created via XunitTestProvider.GetTaskSequence.
        // ReSharper's builtin test providers give a root node of AssemblyLoadTask, which is used
        // to bootstrap a new AppDomain within the external runner process. This method gets called
        // by an instance of CurrentAppDomainHost, the base class of which maintains a dictionary of
        // RemoteTaskRunners, keyed off RunnerID (this class would be one). The constructor of the
        // base class adds a runner called "AssemblyLoadTaskRunner", and adds an instance of either
        // AssemblyLoadTaskRunner or IsolatedAssemblyTaskRunner, depending on a flag passed in from
        // the task runner (which appears to always be true). When the CurrentAppDomainHost encounters
        // the root of the tree, it gets the runner associated with the runner id and executes it.
        // If it's the AssemblyLoadTask, it gets the IsolatedAssemblyTaskRunner, which then creates
        // a new AppDomain with the given assembly codebase. It's also at this point that it specifies
        // whether or not it wants shadow copying turned on or not - this is configurable from the
        // ReSharper options.
        // All of this is a long winded way of saying that we need to create a new AppDomain and
        // specify the shadow copy options the user has specified. Xunit's ExecutorWrapper from the
        // version-independent runner assembly handles all of that for us. Which is why we don't use
        // AssemblyLoadTask
        public override TaskResult Start(TaskExecutionNode node)
        {
            var assemblyTask = (XunitTestAssemblyTask) node.RemoteTask;

            // Set the current directory to the assembly location. This is something that ReSharper's
            // AssemblyLoadTask would do for us, but since we're not using it, we need to do it
            priorCurrentDirectory = Environment.CurrentDirectory;
            var config = Server.GetConfiguration();

            // Use the assembly in the folder that the user has specified, or, if not, use the assembly location
            var assemblyFolder = string.IsNullOrEmpty(config.AssemblyFolder)
                                     ? GetDirectoryName(assemblyTask.AssemblyLocation)
                                     : config.AssemblyFolder;

            Environment.CurrentDirectory = assemblyFolder;

            var assemblyLocation = Path.Combine(assemblyFolder, GetFileName(assemblyTask.AssemblyLocation));
            executorWrapper = new ExecutorWrapper(assemblyLocation, null, config.ShadowCopy);

            return TaskResult.Success;
        }

        private static string GetDirectoryName(string filepath)
        {
            return Path.GetDirectoryName(filepath);
        }

        private static string GetFileName(string filepath)
        {
            return Path.GetFileName(filepath);
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
            foreach (var classNode in node.Children)
            {
                var classTask = (XunitTestClassTask) classNode.RemoteTask;

                var methodTasks = (from methodNode in classNode.Children
                                   select (XunitTestMethodTask) methodNode.RemoteTask).ToList();

                var runnerLogger = new ReSharperRunnerLogger(Server, classTask, methodTasks);
                runnerLogger.ClassStart();

                var methodNames = from task in methodTasks
                                  select task.ShortName;

                var runner = new TestRunner(executorWrapper, runnerLogger);
                runner.RunTests(classTask.TypeName, methodNames.ToList());

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
