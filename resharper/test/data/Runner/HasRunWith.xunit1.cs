using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Extensions;
using Xunit.Sdk;

namespace Foo
{
    [RunWith(typeof(NameBasedTestClassCommand))]
    public class HasRunWith
    {
        public void TestMethod1()
        {
        }

        [Theory]
        [InlineData(42)]
        public void TestMethodWithTheories(int value)
        {
        }

        public class NameBasedTestClassCommand : ITestClassCommand
        {
            private readonly Xunit.Sdk.TestClassCommand command;

            public NameBasedTestClassCommand()
            {
                command = new Xunit.Sdk.TestClassCommand();
            }

            public int ChooseNextTest(ICollection<IMethodInfo> testsLeftToRun)
            {
                return 0;
            }

            public Exception ClassFinish()
            {
                return command.ClassFinish();
            }

            public Exception ClassStart()
            {
                return command.ClassStart();
            }

            public IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo testMethod)
            {
                var commands = command.EnumerateTestCommands(testMethod).ToList();
                if (!commands.Any())
                    commands.Add(new FactCommand(testMethod));
                return commands;
            }

            public IEnumerable<IMethodInfo> EnumerateTestMethods()
            {
                return from method in TypeUnderTest.GetMethods()
                       where IsTestMethod(method)
                       select method;
            }

            public bool IsTestMethod(IMethodInfo testMethod)
            {
                return testMethod.Name.StartsWith("test", StringComparison.InvariantCultureIgnoreCase);
            }

            public object ObjectUnderTest { get { return command.ObjectUnderTest; } }
            public ITypeInfo TypeUnderTest
            {
                get { return command.TypeUnderTest; }
                set { command.TypeUnderTest = value; }
            }
        }
    }
}
