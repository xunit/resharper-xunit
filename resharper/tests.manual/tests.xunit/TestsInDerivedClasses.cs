using Xunit;

namespace tests.xunit.passing
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
            protected string Actual = "working";

            // TEST: RS 6.0 + 6.1 Have a marker with a drop down listing the test in all derived classes
            [Fact]
            public void AbstractBaseTestMethod()
            {
                Assert.Equal("working", Actual);
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

        public class AlsoDerivedFromAbstractBaseClass : AbstractBaseClass
        {
            // TEST: Should be flagged as test
            [Fact]
            public void AnotherDerivedTestMethod()
            {
                Assert.Equal(1, 1);
            }
        }

        public class StillDerivedFromAbstractBaseClass3 : AbstractBaseClass
        {
            [Fact]
            public void YetAnotherDerivedTestMethod()
            {
                Assert.Equal(2, 2);
            }
        }
    }
}
