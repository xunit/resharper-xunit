using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

namespace Foo
{
    public class CustomFactAttributeSkips
    {

        [SkippingFactAttribute]
        public void TestMethod()
        {
        }

        // Here's how you support skip from a custom attribute:
        // 1) Simply set the SkipReason property to a non-null value. TestClassCommand.EnumerateTestCommands checks
        //    this and short-circuits any further method command creation (FixtureCommand, FactCommand) and returns
        //    SkipCommand
        // 2) If you can't set SkipReason from the attribute, or you want to set it more dynamically (e.g. have the
        //    test method decide to skip, perhaps by (yuck) throwing a "skip" exception), you can override
        //    EnumerateTestCommands and return SkipCommand, or a custom command that return SkipResult from Execute
        // 2a) SkipCommand returns null from ToStartXml, so the logger is not notified via TestStart - it just gets
        //     TestSkipped and then TestFinished. Your custom command should probably do the same, but doesn't have to
        //
        // CustomSkipCommand does not return null from ToStartXml, so logger.TestStart, TestSkipped and TestFinished
        // is called. The code originally didn't handle this (http://xunitcontrib.codeplex.com/workitem/4173)
        private class SkippingFactAttribute : FactAttribute
        {
            protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
            {
                yield return new CustomSkipCommand(method);
            }

            private class CustomSkipCommand : FactCommand
            {
                public CustomSkipCommand(IMethodInfo method)
                    : base(method)
                {
                }

                public override MethodResult Execute(object testClass)
                {
                    return new SkipResult(testMethod, DisplayName, "Skipped from a custom command");
                }
            }
        }
    }
}
