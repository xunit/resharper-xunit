using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper
{
    static class XunitTestMethodRunner
    {
        public static TaskResult Execute(IRemoteTaskServer server,
                                         TaskExecutionNode node,
                                         XunitTestMethodTask task)
        {
            XunitTestClassTask @class = (XunitTestClassTask)node.Parent.RemoteTask;
            Type type = XunitTestClassRunner.GetFixtureType(@class, server);
            MethodInfo method;

            try
            {
                method = type.GetMethod(task.TestMethod, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            }
            catch (AmbiguousMatchException)
            {
                server.TaskError(task, "Cannot run ambiguous test method " + type.FullName + "." + task.TestMethod);
                return TaskResult.Error;
            }

            if (method == null)
            {
                server.TaskError(task, string.Format("Cannot find test method '{0}'", task.TestMethod));
                return TaskResult.Error;
            }

            server.TaskProgress(task, "");

            List<MethodResult> results = new List<MethodResult>();
            foreach (ITestCommand command in TestCommandFactory.Make(@class.Command, method))
                results.Add(command.Execute(null));

            List<TaskException> exceptions = new List<TaskException>();

            foreach (MethodResult result in results)
            {
                // xunit doesn't distinguish between STDOUT, STDERR, or DEBUGTRACE, so we'll err
                // on the side of not being so scary and report everything as just STDOUT
                // TODO: What does server.TaskProgress do?
                if(!string.IsNullOrEmpty(result.Output))
                    server.TaskOutput(task, result.Output, TaskOutputType.STDOUT);

                SkipResult skip = result as SkipResult;
                if (skip != null)
                {
                    server.TaskExplain(task, skip.Reason);
                    return TaskResult.Skipped;
                }

                FailedResult failed = result as FailedResult;
                if (failed != null)
                    exceptions.Add(new TaskException(new XunitException(failed.Message, failed.StackTrace)));
            }

            if (exceptions.Count == 0)
                return TaskResult.Success;

            server.TaskException(task, exceptions.ToArray());
            return TaskResult.Error;
        }

        public static TaskResult Finish(IRemoteTaskServer server,
                                        TaskExecutionNode node,
                                        XunitTestMethodTask task)
        {
            return TaskResult.Success;
        }

        public static TaskResult Start(IRemoteTaskServer server,
                                       TaskExecutionNode node,
                                       XunitTestMethodTask task)
        {
            return TaskResult.Success;
        }
    }
}