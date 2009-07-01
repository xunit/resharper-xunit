using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

namespace tests.xunit
{
    namespace ExpectedToFail.NotYetImplemented
    // namespace TestsWithDisplayNames
    {
        public class TestsWithDisplayNames
        {
            // TEST: Name should be reported as attribute value, with no namespaces
            [NamedFact("NameComesFromAttribute")]
            public void ShouldNotSeeThis_NameShouldComeFromAttribute()
            {
                //Assert.Equal(1, 1);
                throw new NotImplementedException();
            }

            // TEST: Name should be reported as attribute value, with no namespaces
            [NamedFact("Name contains spaces")]
            public void ShouldNotSeeThis_NameShouldContainSpaces()
            {
                //Assert.Equal(1, 1);
                throw new NotImplementedException();
            }
        }
    }

    // Unfortunately, xunit 1.1's FactAttribute's Name property can only be
    // set in a derived class. And it's not passed into the test commands either.
    // This is fixed in xunit 1.5
    public class NamedFactAttribute : FactAttribute
    {
        public NamedFactAttribute(string name)
        {
            Name = name;
        }

        protected override IEnumerable<Xunit.Sdk.ITestCommand> EnumerateTestCommands(System.Reflection.MethodInfo method)
        {
            yield return new TestCommand(method, Name);
        }
    }
}
