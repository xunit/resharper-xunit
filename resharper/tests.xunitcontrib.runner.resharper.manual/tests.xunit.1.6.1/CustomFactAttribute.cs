using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Extensions;
using Xunit.Sdk;

namespace tests.xunit
{
    namespace CustomAttributes
    {
        public class MyFactAttribute : FactAttribute
        {
            // Do nothing
        }

        public class CustomFactAttribute
        {
            // TEST: Should be flagged as a test class
            [MyFact]
            public void MyTestMethod()
            {
                Assert.Equal(1, 1);
            }
        }

        public class MyTheoryAttribute : TheoryAttribute
        {
            public int Repeat { get; set; }

            protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
            {
                for (int i = 0; i < Repeat; i++)
                    yield return new FactCommand(method);
            }
        }

        public class CustomTheoryAttribute
        {
            // TEST: Should be flagged as a test
            // TEST: Should output CustomTheoryAttribute.MyTestMethod 5 times
            [MyTheory(Repeat = 5)]
            public void MyTestMethod()
            {
                Console.WriteLine("CustomTheoryAttribute.MyTestMethod");
            }
        }
    }
}