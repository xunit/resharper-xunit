using System;
using System.IO;
using System.Reflection;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit.Sdk;

namespace Xunit.Runner.ReSharper
{
    static class XunitTestClassRunner
    {
        public static TaskResult Execute(IRemoteTaskServer server,
                                         TaskExecutionNode node,
                                         XunitTestClassTask @class)
        {
            return TaskResult.Success;
        }

        public static TaskResult Finish(IRemoteTaskServer server,
                                        TaskExecutionNode node,
                                        XunitTestClassTask @class)
        {
            if (@class.Command != null)
            {
                Exception ex = @class.Command.ClassFinish();
                if (ex != null)
                {
                    server.TaskException(@class, new TaskException[] { new TaskException(ex) });
                    return TaskResult.Exception;
                }
            }

            return TaskResult.Success;
        }

        public static Type GetFixtureType(XunitTestClassTask @class,
                                          IRemoteTaskServer server)
        {
            string assemblyLocation = @class.AssemblyLocation;
            if (!File.Exists(assemblyLocation))
            {
                server.TaskError(@class, string.Format("Cannot load assembly from {0}: file does not exist", assemblyLocation));
                return null;
            }

            AssemblyName assemblyName = AssemblyName.GetAssemblyName(assemblyLocation);
            if (assemblyName == null)
            {
                server.TaskError(@class, string.Format("Cannot load assembly from {0}: not a .NET assembly", assemblyLocation));
                return null;
            }

            Assembly assembly = Assembly.Load(assemblyName);
            if (assembly == null)
            {
                server.TaskError(@class, string.Format("Cannot load assembly from {0}", assemblyLocation));
                return null;
            }

            return assembly.GetType(@class.TypeName);
        }

        public static TaskResult Start(IRemoteTaskServer server,
                                       TaskExecutionNode node,
                                       XunitTestClassTask @class)
        {
            Type type = GetFixtureType(@class, server);
            if (type == null)
                return TaskResult.Error;

            server.TaskProgress(@class, "");

            @class.Command = TestClassCommandFactory.Make(type);
            if (@class.Command == null)
            {
                server.TaskError(@class, "Could not create ITestClassCommand for typeInfo " + type.FullName);
                return TaskResult.Error;
            }

            Exception ex = @class.Command.ClassStart();
            if (ex != null)
            {
                @class.Command = null;
                server.TaskException(@class, new TaskException[] { new TaskException(ex) });
                return TaskResult.Exception;
            }

            return TaskResult.Success;
        }
    }
}