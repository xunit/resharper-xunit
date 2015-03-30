using System;
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
            // TODO: Logging for 8.2
            get { return false; }
        }

        public static void Log(string format, params object[] args)
        {
            // TODO: Logging for 8.2
        }

        public static void LogVerbose(string format, params object[] args)
        {
            // TODO: Logging for 8.2
        }

        private static void LogMessage(LogLevel logLevel, string format, params object[] args)
        {
            // TODO: Logging for 8.2
        }

        public static void LogException(Exception exception)
        {
            // TODO: Logging for 8.2
        }

        public static string Format(this ITestCase testCase)
        {
            if (!IsEnabled)
                return "Meh. Logging not enabled. Don't care";

            return string.Format("{0}.{1} - {2} ({3})", testCase.TestMethod.TestClass.Class.Name,
                testCase.TestMethod.Method.Name, testCase.DisplayName ?? string.Empty, testCase.UniqueID);
        }
    }
}