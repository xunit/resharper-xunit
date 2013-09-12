using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public static class ServerMessage
    {
        private static readonly IDictionary<ServerAction, Func<object[], string>> Formatters = new Dictionary<ServerAction, Func<object[], string>>
        {
            {ServerAction.TaskOutput, FormatTwoParametersReverseOrder},
            {ServerAction.TaskFinished, FormatTwoParametersReverseOrder},
            {ServerAction.TaskException, FormatException}
        };

        public static string Format(ServerAction serverAction, params object[] values)
        {
            var content = FormatContent(serverAction, values);
            return string.Format("{0}{1}{2}", serverAction, string.IsNullOrEmpty(content) ? "" : ": ", content);
        }

        private static string FormatContent(ServerAction serverAction, object[] args)
        {
            Func<object[], string> formatter;
            if (!Formatters.TryGetValue(serverAction, out formatter))
                formatter = DefaultFormat;
            return formatter(args);
        }

        private static string DefaultFormat(object[] args)
        {
            return args.Length > 0 ? string.Format("{0}", args) : string.Empty;
        }
        private static string FormatTwoParametersReverseOrder(object[] args)
        {
            return string.Format("{1} - <{0}>", args);
        }

        private static string FormatException(object[] args)
        {
            var exceptions = args[0] as IEnumerable<TaskException>;
            if (exceptions != null)
                return FormatTaskExceptions(exceptions);
            var exception = args[0] as Exception;
            if (exception != null)
                return FormatException(exception);
            throw new InvalidOperationException("Unexpected args!");
        }

        private static string FormatTaskExceptions(IEnumerable<TaskException> exceptions)
        {
            var formattedExceptions = from exception in exceptions
                                      select FormatException(exception.Type, exception.Message);
            return string.Join(Environment.NewLine, formattedExceptions.ToArray());
        }

        private static string FormatException(Exception exception)
        {
            var exceptions = new List<string>();
            while (exception != null)
            {
                exceptions.Add(FormatException(exception.GetType().FullName, exception.Message));
                exception = exception.InnerException;
            }

            return string.Join(Environment.NewLine, exceptions.ToArray());
        }

        private static string FormatException(string type, string message)
        {
            return string.Format("{0}: {1}", type, message);
        }
    }
}