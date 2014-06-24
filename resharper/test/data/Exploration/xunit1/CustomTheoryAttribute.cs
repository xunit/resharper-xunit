using System.Collections.Generic;
using Xunit;
using Xunit.Extensions;
using Xunit.Sdk;

namespace Foo
{
    public class MyTheoryAttribute : TheoryAttribute
    {
        public int Repeat { get; set; }

        protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
        {
            for (int i = 0; i < Repeat; i++)
                yield return new FactCommand(method);
        }
    }

    public class Tests
    {
        [MyTheory(Repeat = 5)]
        public void TestMethod()
        {
        }
    }
}
