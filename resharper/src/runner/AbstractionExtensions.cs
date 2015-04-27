using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.TaskRunnerFramework;
using JetBrains.Util;
using Xunit.Abstractions;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public static class AbstractionExtensions
    {
        public static TaskException[] ConvertExceptions(this IFailureInformation failure, out string simplifiedMessage)
        {
            var exceptions = new List<TaskException>();

            for (var i = 0; i < failure.ExceptionTypes.Length; i++)
            {
                // There's a bug in 2.0 that parses an exception incorrectly, so let's do this defensively
                var type = failure.ExceptionTypes.Length > i ? failure.ExceptionTypes[i] : string.Empty;

                // Strip out the xunit assert methods from the stack traces by taking
                // out anything in the Xunit.Assert namespace
                var stackTraces = (failure.StackTraces.Length > i && failure.StackTraces[i] != null) ? failure.StackTraces[i]
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(s => !s.Contains("Xunit.Assert")).Join(Environment.NewLine) : string.Empty;

                var message = failure.Messages.Length > i ? failure.Messages[i] : string.Empty;

                exceptions.Add(new TaskException(type, message, stackTraces));
            }

            // Simplified message - if it's an xunit native exception (most likely an assert)
            // only include the exception message, otherwise include the exception type
            var exceptionMessage = failure.Messages.Length > 0 ? failure.Messages[0] : "<No exception message>";
            var safeExceptionType = failure.ExceptionTypes.Length > 0 ? failure.ExceptionTypes[0] : "<Unknown exception type>";
            var exceptionType = safeExceptionType.StartsWith("Xunit") ? string.Empty : (safeExceptionType + ": ");
            simplifiedMessage = exceptionMessage.StartsWith(safeExceptionType) ? exceptionMessage : exceptionType + exceptionMessage;

            return exceptions.ToArray();
        }

        public static string FullyQualifiedName(this ITestMethod testMethod)
        {
            return string.Format("{0}.{1}", testMethod.TestClass.Class.Name, testMethod.Method.Name);
        }

        public static string FullyQualifiedName(this ITestCase testCase)
        {
            return testCase.TestMethod.FullyQualifiedName();
        }
    }
}