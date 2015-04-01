using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Extensions;
using Xunit.Sdk;

namespace Foo
{
    public class MethodWithRepeatedNames
    {
        [Repeat(5)]
        public void DoTests()
        {
        }
    }

    public class RepeatAttribute : FactAttribute
    {
        private readonly int iterations;

        public RepeatAttribute(int iterations)
        {
            this.iterations = iterations;
        }

        protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
        {
            for(int i = 0; i < iterations; i++)
            {
                foreach(var command in base.EnumerateTestCommands(method))
                    yield return command;
            }
        }
    }
}
