using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

namespace XunitContrib.Runner.Silverlight.Toolkit
{
    internal class ExceptionInterceptingTestCommandFactory
    {
        public static IEnumerable<ITestCommand> Make(ITestClassCommand classCommand, IMethodInfo method)
        {
            foreach (var testCommand in classCommand.EnumerateTestCommands(method))
            {
                var wrappedCommand = testCommand;

                wrappedCommand = new BeforeAfterCommand(wrappedCommand, method.MethodInfo);

                if (testCommand.ShouldCreateInstance)
                    wrappedCommand = new LifetimeCommand(wrappedCommand, method);

                wrappedCommand = new TimedCommand(wrappedCommand);
                wrappedCommand = new ExceptionInterceptingCommand(wrappedCommand, method);
                wrappedCommand = new ExceptionAndOutputCaptureCommand(wrappedCommand, method);

                // Note that we don't use a TimeoutCommand - we'll let the Silverlight framework handle that

                yield return wrappedCommand;
            }
        }
    }
}