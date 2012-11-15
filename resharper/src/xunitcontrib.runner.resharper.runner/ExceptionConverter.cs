using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public static class ExceptionConverter
    {
        // This is messy, but necessary. xunit reports exceptions and inner exceptions through 3 parameters,
        // the type name of the outermost exception, the messages and type names of all exceptions concatenated
        // together, and the stack traces of all exceptions concatenated. The messages are separated by newlines,
        // and repeat the 4 character string "----" to denote depth. E.g.:
        // "typename : message\r\n----typename : message\r\n--------typename : message"
        // The stack traces are just concatenated with "----- Inner Stack Trace -----" and newlines
        // Resharper wants each inner exception listed separately, and a message that is the short name
        // of the exception type plus the exception's message. Oh, and don't bother showing TargetInvocationExceptions
        // either.
        public static TaskException[] ConvertExceptions(string outermostExceptionType, string nestedExceptionMessages, string nestedStackTraces, out string simplifiedMessage)
        {
            nestedStackTraces = nestedStackTraces ?? string.Empty;

            if(outermostExceptionType == "Xunit.Sdk.AfterTestException")
            {
                return ConvertAfterTestException(outermostExceptionType, nestedExceptionMessages, nestedStackTraces,
                                                 out simplifiedMessage);
            }

            if (outermostExceptionType.StartsWith("Xunit"))
            {
                return ConvertAssertException(outermostExceptionType, nestedExceptionMessages, nestedStackTraces,
                                              out simplifiedMessage);
            }

            string[] stackTraces = Regex.Split(nestedStackTraces, @"\r*\n----- Inner Stack Trace -----\r*\n");

            var exceptions = new List<TaskException>();
            Match match = Regex.Match(nestedExceptionMessages, @"-*\s*(?<type>.*?) :\s*(?<message>.*?)((\r\n-)|\z)", 
                RegexOptions.ExplicitCapture | RegexOptions.Multiline | RegexOptions.Singleline);
            for (int i = 0; match.Success; i++, match = match.NextMatch())
                exceptions.Add(CreateTaskException(match.Groups["type"].Value, match.Groups["message"].Value.Trim(), stackTraces[i]));

            // If the exceptions weren't formatted with xunit's ExceptionUtility, the message won't match
            // the pattern "type : message", and we get no exceptions. If that happens, just create one
            // exception from what we've got. I really wish xunit had a less fudgy way of passing around
            // exception details
            if (exceptions.Count == 0)
                exceptions.Add(CreateTaskException(outermostExceptionType, nestedExceptionMessages, nestedStackTraces));

            // Don't show TargetInvocationExceptions - they're not very useful to the end user...
            if(exceptions.Count > 1 && exceptions[0].Type == "System.Reflection.TargetInvocationException")
                exceptions.RemoveAt(0);

            // Note that type here is the fully qualified type
            simplifiedMessage = exceptions[0].Type + ": " + exceptions[0].Message;

            return exceptions.ToArray();
        }

        // The after test exception is thrown when one or more exceptions are thrown in the After method
        // of custom BeforeAfterTestAttributes. These attributes are executed by the BeforeAfterCommand,
        // which loops over all attributes calling Before(). It then executes the test, and loops over
        // the attributes in reverse order calling After().
        // If any After method throws an exception, the exception is collected and added to an instance
        // of AfterTestException. This list of collected exceptions is then serialised into the stack
        // trace of the AfterTestException, in the form "typeName thrown: message\r\nstackTrace", with
        // a newline between exceptions. Inner exceptions are not serialised.
        // The AfterTestException is then passed to the test runner logger, the same as for a normal
        // exception (it just has a stack trace that represents a list of exceptions, rather than a
        // tree)
        // If an exception is thrown in the Before method, then it just escapes and gets wrapped as
        // per a "normal" exception (assert or other exception thrown in a test).
        // If an exception is thrown in both a Before and an After method, the Before exception is
        // silently ignored.
        // e.g message = "Xunit.Sdk.AfterTestException : One or more exceptions were thrown from After methods during test cleanup"
        /* serialisedStackTraces = 
System.NotImplementedException thrown: message from NotImplementedException
   at XunitContrib.Runner.ReSharper.RemoteRunner.Tests.ExceptionConverterTests.GenerateSingleException(String message) in C:\Users\matt\Projects\OpenSource\xunitcontrib\resharper\tests.xunitcontrib.runner.resharper.runner.4.5\ExceptionConverterTests.cs:line 237

System.NotImplementedException thrown: message from NotImplementedException no.2
   at XunitContrib.Runner.ReSharper.RemoteRunner.Tests.ExceptionConverterTests.GenerateSingleException2() in C:\Users\matt\Projects\OpenSource\xunitcontrib\resharper\tests.xunitcontrib.runner.resharper.runner.4.5\ExceptionConverterTests.cs:line 249

System.NotImplementedException thrown: message from NotImplementedException no.3
   at XunitContrib.Runner.ReSharper.RemoteRunner.Tests.ExceptionConverterTests.GenerateSingleException3() in C:\Users\matt\Projects\OpenSource\xunitcontrib\resharper\tests.xunitcontrib.runner.resharper.runner.4.5\ExceptionConverterTests.cs:line 261
         */
        private static TaskException[] ConvertAfterTestException(string typeName, string message, string serialisedStackTraces, out string simplifiedMessage)
        {
            simplifiedMessage = typeName + ": " + Regex.Match(message, "^.*: (?<message>.*$)").Groups["message"].Value;

            var exceptions = new List<TaskException>();
            // The match for "   " in the stack trace capture is matching the three spaces before the "at". We
            // can't rely on the "at" because the CLR localises stack traces. Dodgy, eh?
            Match match = Regex.Match(serialisedStackTraces, @"(?<type>[^\n]*) thrown: (?<message>.*?)\r\n(?<stackTrace>   .*?)(\r\n\r\n|\Z)",
                                      RegexOptions.ExplicitCapture | RegexOptions.Singleline);
            while (match.Success)
            {
                exceptions.Insert(0, CreateTaskException(match.Groups["type"].Value, match.Groups["message"].Value, match.Groups["stackTrace"].Value));
                match = match.NextMatch();
            }
            return exceptions.ToArray();
        }

        // Just to make life interesting, if the exception is an xunit AssertionException, then xunit
        // doesn't add the type name to the message. This is fine if the outermost exception derives
        // from the assert exception, but what happens if it's not - e.g. it's caught and then rethrown?
        // TODO: Do we want to support xunit exceptions with inner exceptions?
        // TODO: Support inner exceptions that are xunit assert exceptions?
        private static TaskException[] ConvertAssertException(string outermostExceptionType, string nestedExceptionMessages, string nestedStackTraces, out string simplifiedMessage)
        {
            var exceptions = new List<TaskException>();
            simplifiedMessage = nestedExceptionMessages;
            exceptions.Add(CreateTaskException(outermostExceptionType, nestedExceptionMessages, nestedStackTraces));
            return exceptions.ToArray();
        }

        private static TaskException CreateTaskException(string fullTypeName, string message, string stackTrace)
        {
            return new TaskException(fullTypeName, message, stackTrace);
        }
    }
}