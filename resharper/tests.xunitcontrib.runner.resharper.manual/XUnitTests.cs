using System;
using Xunit;
using Xunit.Extensions;

// XUnit runner runs 5 tests in 3 fixtures:
// ConcreteBaseClass (1 test)
// DerivedFromConcreteBaseClass (2 tests)
// DerivedFromAbstractBaseClass (2 tests)

namespace tests.xunitcontrib.runner.resharper.manual
{
    namespace xunit
    {
        public class ConcreteBaseClass
        {
            [Fact]
            public void BaseTestMethodShouldBeFlaggedInOwnClassAndDerivedClass()
            {
                Assert.Equal(1, 1);
            }
        }

        public class DerivedFromConcreteBaseClass : ConcreteBaseClass
        {
            // Should get 2 tests - this one and one from ConcreteBaseClass
            [Fact]
            public void DerivedTestMethodShouldBeFlaggedInOwnClass()
            {
                Assert.True(2 == 2);
            }
        }

        public abstract class AbstractBaseClass
        {
            [Fact]
            public void TestMethodInAbstractBaseClassShouldNotBeFlagged()
            {
                Assert.True(2 == 2);
            }
        }

        public class DerivedFromAbstractBaseClass : AbstractBaseClass
        {
            // Should get 2 tests - this one and one from AbstractBaseClass
            [Fact]
            public void DerivedTestMethodShouldBeFlaggedInOwnClass()
            {
                Assert.True(2 == 2);
            }
        }

        // Should not be flagged
        public class ParentClass
        {
            // Should be flagged
            public class NestedClass
            {
                [Fact]
                public void NestedTestShouldBeFlagged()
                {
                    Assert.Equal(1, 1);
                }
            }
        }

        public class FailingTests
        {
            [Fact]
            public void Exception()
            {
                throw new Exception("this test should fail");
            }

            [Fact]
            public void AssertFails()
            {
                Assert.Equal("this test", "should fail");
            }
        }

        class PrivateClass
        {
            [Fact]
            public void PublicTestMethodOnPublicClassShouldNotBeFlagged()
            {
                throw new NotImplementedException();
            }

            [Fact]
            internal void InternalTestMethodShouldNotBeFlagged()
            {
                throw new NotImplementedException();
            }

            [Fact]
            protected void ProtectedTestMethodShouldNotBeFlagged()
            {
                throw new NotImplementedException();
            }

            [Fact]
            private void PrivateTestMethodShouldNotBeFlagged()
            {
                throw new NotImplementedException();
            }
        }

        internal class InternalClass
        {
            [Fact]
            public void PublicTestMethodOnInternalClassShouldNotBeFlagged()
            {
                throw new NotImplementedException();
            }

            [Fact]
            internal void InternalTestMethodShouldNotBeFlagged()
            {
                throw new NotImplementedException();
            }

            [Fact]
            protected void ProtectedTestMethodShouldNotBeFlagged()
            {
                throw new NotImplementedException();
            }

            [Fact]
            private void PrivateTestMethodShouldNotBeFlagged()
            {
                throw new NotImplementedException();
            }
        }

        public class OutputTestClass
        {
            [Fact]
            public void StdOutTest()
            {
                Console.WriteLine("Hello world from stdout");
            }

            [Fact]
            public void StdErrTest()
            {
                Console.Error.WriteLine("Hello world from stderr");
            }
        }

        public class SkippedClass
        {
            [Fact(Skip = "This is the skip reason")]
            public void SkippedTest()
            {
                throw new NotImplementedException();
            }
        }

        public class TestClassWithTraitAttribute
        {
            [Fact, Trait("name", "value")]
            public void TestHasNameTrait()
            {
                Assert.Equal(1, 1);
            }

            [Fact, Trait("category", "fancyCategory")]
            public void TestMethodHasCategoryTrait()
            {
                Assert.Equal(1, 1);
            }
        }

        public class TheoryTestClass
        {
            [Theory]
            [InlineData("hello", 5)]
            public void SingleLineTheoryTest(string value, int expectedLength)
            {
                //Console.WriteLine("String: " + value + " with expected length of " + expectedLength);
                Assert.Equal(expectedLength, value.Length);
            }

            [Theory]
            [InlineData("hello", 5)]
            [InlineData("hello world", 11)]
            [InlineData("cheese", 6)]
            public void MultipleLineTheoryTest(string value, int expectedLength)
            {
                //Console.WriteLine("String: " + value + " with expected length of " + expectedLength);
                Assert.Equal(expectedLength, value.Length);
            }

            [Theory]
            [InlineData("failing", 0)]
            [InlineData("works", 5)]
            [InlineData("another failure", 2)]
            public void FailingTheoryTest(string value, int expectedLength)
            {
                //Console.WriteLine("String: " + value + " with expected length of " + expectedLength);
                Assert.Equal(expectedLength, value.Length);
            }
        }
    }
}