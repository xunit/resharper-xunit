using Xunit;

namespace tests.xunit
{
    namespace TestsInNestedClasses
    {
        // TEST: Should not get flagged as test class
        // TEST: Solution Wide Analysis does not mark this as unused
        //       (in non-test code, it would get flagged as never instantiated)
        public class ParentClass
        {
            // TEST: Should be flagged and run
            // TEST: Does not display ParentClass
            public class NestedClass1
            {
                [Fact]
                public void NestedTestIsValid()
                {
                    Assert.Equal(1, 1);
                }
            }
        }

        // TEST: None of these should be flagged as test classes (except NestedClass)
        // TEST: Solution Wide Analysis does not mark any of these classes as unused
        public class DeeplyNestedParentClass
        {
            public class DeeplyNestedClass
            {
                public class ReallyDeeplyNestedClass
                {
                    public class RidiculouslyDeeplyNestedClass
                    {
                        // TEST: Should be flagged and run
                        // TEST: Does not display any of the parent classes
                        public class NestedClass2
                        {
                            [Fact]
                            public void DeeplyNestedTestIsValid()
                            {
                                Assert.Equal(1, 1);
                            }
                        }
                    }
                }
            }
        }
    }
}