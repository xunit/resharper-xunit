using Xunit;

namespace tests.xunit
{
    namespace TestsInDerivedClasses
    {
        public class ConcreteBaseClass
        {
            // TEST: Should be flagged as test method
            [Fact]
            public void BaseTestMethod()
            {
                Assert.Equal(1, 1);
            }
        }

        // TEST: Should have 2 tests; should also include "ConcreteBaseClass.BaseTestMethod"
        public class DerivedFromConcreteBaseClass : ConcreteBaseClass
        {
            // TEST: Should be flagged as test method
            [Fact]
            public void DerivedTestMethod()
            {
                Assert.Equal(1, 1);
            }
        }

        public abstract class AbstractBaseClass
        {
            // TEST: Should not be flagged as test method
            [Fact]
            public void AbstractBaseTestMethod()
            {
                Assert.Equal(1, 1);
            }
        }

        // TEST: Should have 2 tests; should also include "AbstractBaseClass.AbstractBaseTestMethod"
        public class DerivedFromAbstractBaseClass : AbstractBaseClass
        {
            // TEST: Should be flagged as test
            [Fact]
            public void DerivedTestMethod()
            {
                Assert.Equal(1, 1);
            }
        }
    }
}
