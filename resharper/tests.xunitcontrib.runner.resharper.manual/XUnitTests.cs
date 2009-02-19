using System;
using System.Diagnostics;
using Xunit;
using Xunit.Extensions;
using Xunit.Sdk;

// XUnit runner runs 5 tests in 3 fixtures:
// ConcreteBaseClass (1 test)
// DerivedFromConcreteBaseClass (2 tests)
// DerivedFromAbstractBaseClass (2 tests)

namespace tests
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

        // ReSharper disable MemberCanBeMadeStatic
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
        // ReSharper restore MemberCanBeMadeStatic

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

        public class NamedFactAttribute : FactAttribute
        {
            public NamedFactAttribute(string name)
            {
                Name = name;
            }

            // Ideally, we shouldn't have to do this - FactAttribute should use Name itself
            protected override System.Collections.Generic.IEnumerable<ITestCommand> EnumerateTestCommands(System.Reflection.MethodInfo method)
            {
                yield return new TestCommand(method, Name);
            }
        }

        public class NamedTests
        {
            [NamedFactAttribute("NameComesFromAttribute")]
            public void ShouldNotSeeThis_NameShouldComeFromAttribute()
            {
                throw new NotImplementedException("Named tests currently not implemented");
            }

            [NamedFactAttribute("Name contains spaces")]
            public void ShouldNotSeeThis_NameShouldContainSpaces()
            {
                throw new NotImplementedException("Named tests currently not implemented");
            }
        }

        public class TestClassWithTraitAttribute
        {
            [Fact, Trait("name", "value")]
            public void TestHasNameTrait()
            {
                throw new NotImplementedException("What should this do?");
            }

            [Fact, Trait("category", "fancyCategory")]
            public void TestMethodHasCategoryTrait()
            {
                throw new NotImplementedException("Categories not yet supported");
            }
        }

        public class TheoryTestClass
        {
            //public TheoryTestClass()
            //{
            //    System.Threading.Thread.Sleep(5000);
            //}

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
                System.Threading.Thread.Sleep(1000);
                Assert.Equal(expectedLength, value.Length);
            }
        }

        namespace SupposedToFail
        {
            public class FailingTests
            {
                [Fact]
                public void FailsDueToThrownException()
                {
                    throw new Exception("this test should fail");
                }

                [Fact]
                public void FailsDueToAssert()
                {
                    Assert.Equal("this test", "should fail");
                }

                [Fact]
                public void FailsDueToDebugAssert()
                {
                    Debug.Assert(1 == 0, "message - supposed to fail", "detailed message - supposed to fail");
                }

                [Fact]
                public void FailsDueToTraceAssert()
                {
                    Trace.Assert(1 == 0, "message - supposed to fail", "detailed message - supposed to fail");
                }
            }

            public class FailingTheoryTests
            {
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
}