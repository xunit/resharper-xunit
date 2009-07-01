using Xunit;

namespace tests.xunit
{
    namespace TestsInNestedClasses
    {
        // TEST: Should not get flagged
        // TEST: Solution Wide Analysis does not mark this as unused
        //       (in non-test code, it would get flagged as never instantiated)
        public class ParentClass
        {
            // TEST: Should be flagged and run
            // TEST: Does not display ParentClass
            public class NestedClass
            {
                [Fact]
                public void NestedTestIsValid()
                {
                    Assert.Equal(1, 1);
                }
            }
        }
    }
}