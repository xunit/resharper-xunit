using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Extensions;
using Xunit.Sdk;

namespace tests.xunit.passing
{
    namespace CustomAttributes
    {
        public class ThatchAttribute : FactAttribute
        {
            protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
            {
                return base.EnumerateTestCommands(method).Select(c => new ThatchCommand(c,method));
            }
        }

        public class ThatchCommand : DelegatingTestCommand
        {
            private readonly IMethodInfo method;

            public ThatchCommand(ITestCommand testCommand, IMethodInfo method) : base(testCommand)
            {
                this.method = method;
            }

            public override MethodResult Execute(object testClass)
            {
                return new SkipResult(method, DisplayName, "Because we want to");
            }
        }

        public class UsesThatchAttribute
        {
            [Thatch]
            public void Thatch()
            {
                Assert.Equal(42, 42);
            }

            [Fact(Skip = "Sausages")]
            public void FactSkipped()
            {


                string value = null;

if (value == null)
{












                    value = GetDefaultValue();



                    Console.WriteLine(value);
                }



var s = "Hello world";





                s = s.ToUpper();

                Console.WriteLine(value);
            }

            private string GetDefaultValue()
            {
                throw new NotImplementedException();
            }
        }

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