using System;
using System.Collections.Generic;
using System.Linq;
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

            var results = new List<MethodResult>();
            foreach (var command in TestCommandFactory.Make(@class.Command, method))
            {
                server.TaskProgress(task, string.Format("Invoking {0}", command.DisplayName));
                var result = command.Execute(null);
                server.TaskProgress(task, string.Empty);

                // xunit doesn't distinguish between STDOUT, STDERR, or DEBUGTRACE, so we'll err
                // on the side of not being so scary and report everything as just STDOUT
                if (!string.IsNullOrEmpty(result.Output))
                    server.TaskOutput(task, result.Output, TaskOutputType.STDOUT);

                results.Add(result);
            }

            var taskExceptions = new List<TaskException>();
            foreach (var result in results)
            {
                var skip = result as SkipResult;
                if (skip != null)
                {
                    server.TaskExplain(task, skip.Reason);
                    server.TaskFinished(task, skip.Reason, TaskResult.Skipped);
                    return TaskResult.Skipped;
                }

                // TODO: How does xunit handle inner exceptions?s
                var failed = result as FailedResult;
                if (failed != null)
                {
                    //server.TaskOutput(task, "TypeName: " + result.TypeName, TaskOutputType.STDOUT);
                    //server.TaskOutput(task, "MethodName: " + result.MethodName, TaskOutputType.STDOUT);
                    //server.TaskOutput(task, "Message: " + failed.Message, TaskOutputType.STDOUT);
                    //server.TaskOutput(task, "StackTrace: " + failed.StackTrace, TaskOutputType.STDOUT);

                    // This is going to be a bit messy to sort out. Basically, result.TypeName is the name of the
                    // type under test. MethodName is the name of the test method.
                    // Message is really more interesting. If it's not an AssertException, the full typename is
                    // prepended to the message. Inner exceptions are appended to the message, with multiples of
                    // "----" added between each inner exception message.
                    // The stack trace also includes the stack traces of any inner exceptions, with the
                    // "----- Inner Stack Trace -----" marker added.
                    // To get parity with the nunit runner, we need to match against the start of the message to
                    // see if it's a fully qualified type ("string.string : ") and if so, strip out the unnecessary
                    // namespaces. We should probably then reconstruct the types, messages and stack traces of
                    // all inner exceptions, and add them as separate TaskExceptions. Phew.
                    // Check out TaskExecutor.ConvertExceptions
                    taskExceptions.Add(new TaskException(new XunitException(failed.Message, failed.StackTrace)));
                }
            }

            if (taskExceptions.Count == 0)
                return TaskResult.Success;

            server.TaskException(task, taskExceptions.ToArray());
            server.TaskFinished(task, taskExceptions[0].Message, TaskResult.Exception);
            return TaskResult.Exception;
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