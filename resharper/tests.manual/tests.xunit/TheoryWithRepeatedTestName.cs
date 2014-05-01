using System.Collections.Generic;
using Xunit;

namespace tests.xunit.eyeball
{
    public class TheoryWithRepeatedTestName
    {
        public class Data
        {
            public string Value;

            public override string ToString()
            {
                // Make sure that when xUnit tries to display the parameters,
                // it will be the same no matter the value
                // ToString by default returns GetType().FullName, let's just
                // make this obvious, and clean up the namespace we're using
                return "SomeNamespace.Data";
            }
        }

        public static IEnumerable<object[]> TheoryData
        {
            get
            {
                yield return new object[] { new Data { Value = "Stuff" } };
                yield return new object[] { new Data { Value = "Stuff" } };
                yield return new object[] { new Data { Value = "Stuff" } };
                yield return new object[] { new Data { Value = "Stuff" } };
            }
        }

        // TEST: xUnit calls ToString on the parameter, but if ToString returns
        // the same value as a previous theory, multiple rows can have the same
        // test "name" - e.g. ShouldDisplayFourTests(data: Namespace.Data)
        // Test that subsequent calls get appended with a number - e.g.
        // ShouldDisplayFourTests(data: Namespace.Data)
        // ShouldDisplayFourTests(data: Namespace.Data) [2]
        // ShouldDisplayFourTests(data: Namespace.Data) [3]
        // etc
        [Theory]
        [MemberData("TheoryData")]
        public void ShouldDisplayFourTests(Data data)
        {
        }
    }
}