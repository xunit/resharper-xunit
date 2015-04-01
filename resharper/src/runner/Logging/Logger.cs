using System;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit.Abstractions;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Logging
{
    public static class Logger
    {
        // Matches ReSharper 9's SimpleLogger.LogLevel
        public enum LogLevel
        {
            Normal,
            Verbose
        }

        public static bool IsEnabled
        {
            get { return TaskExecutor.Logger.IsVerboseEnabled(); }
        }

        public static void Log(string format, params object[] args)
        {
            LogMessage(LogLevel.Normal, format, args);
        }

        public static void LogVerbose(string format, params object[] args)
        {
            LogMessage(LogLevel.Verbose, format, args);
        }

        private static void LogMessage(LogLevel logLevel, string format, params object[] args)
        {
            TaskExecutor.Logger.LogMessage((SimpleLogger.LogLevel)logLevel, format, args);
        }

        public static void LogException(Exception exception)
        {
            TaskExecutor.Logger.LogException(exception);
        }

        public static string Format(this ITestCase testCase)
        {
            if (!IsEnabled)
                return "Meh. Logging not enabled. Don't care";

            return string.Format("«{0}.{1} - {2} ({3})»", testCase.TestMethod.TestClass.Class.Name,
                testCase.TestMethod.Method.Name, testCase.DisplayName ?? string.Empty, testCase.UniqueID);
        }
    }
}