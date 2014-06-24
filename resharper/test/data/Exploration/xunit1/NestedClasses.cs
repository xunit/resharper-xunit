using Xunit;

namespace Foo
{
    public class ParentClass
    {
        public class NestedClass1
        {
            [Fact]
            public void TestMethod()
            {
            }
        }
    }

    public class DeeplyNestedParentClass
    {
        public class DeeplyNestedClass
        {
            public class ReallyDeeplyNestedClass
            {
                public class RidiculouslyDeeplyNestedClass
                {
                    public class NestedClass2
                    {
                        [Fact]
                        public void TestMethod()
                        {
                        }
                    }
                }
            }
        }
    }
}
