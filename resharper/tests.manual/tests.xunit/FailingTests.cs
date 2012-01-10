using System;
using System.Diagnostics;
using System.IO;
using Xunit;
using Xunit.Extensions;

namespace tests.xunit.failing
{
    namespace ExpectedToFail
    {
        public class FailingTests
        {
            // TEST: Should fail
            // TEST: Should display "Failed: System.Exception: this test should fail" as tree view status
            // TEST: Should display System.Exception: this test should fail + stack trace as main view
            // BUG: Does not display shortname in status view
            [Fact]
            public void FailsDueToThrownException()
            {
                throw new Exception("this test should fail");
            }

            // TEST: Should fail
            // TEST: Nunit displays "Failed: AssertionException: Expected string length 9 but was 11. Strings differ at index 0. Expected "this test" But was:
            // TEST: Nunit displays "NUnit.Framework.AssertionException: Expected string length 9..."
            [Fact]
            public void FailsDueToAssert()
            {
                Assert.Equal("this test", "should fail");
            }

            // TEST: Should fail
            [Fact]
            public void FailsDueToDebugAssert()
            {
                Debug.Assert(1 == 0, "message - supposed to fail", "detailed message - supposed to fail");
            }

            // TEST: Should fail
            [Fact]
            public void FailsDueToTraceAssert()
            {
                // Note that this method requires the "Define TRACE constant" project property ticked
                Trace.Assert(1 == 0, "message - supposed to fail", "detailed message - supposed to fail");
            }
        }

        public class NestedExceptions
        {
            // TEST: Should fail
            // TEST: Should display "Failed: System.InvalidProgramException: invalid program exception message" in tree view status
            /* TEST: Should display:
System.InvalidOperationException: Invalid operation exception message

at tests.reference.nunit.ExpectedToFail.NestedExceptions.CallAnotherMethod() in FailingTests.cs: line 60
at tests.reference.nunit.ExpectedToFail.NestedExceptions.CallSomeMethod() in FailingTests.cs: line 55
at tests.reference.nunit.ExpectedToFail.NestedExceptions.HasInnerException() in FailingTests.cs: line 45 

System.InvalidProgramException: Invalid program exception message

at tests.reference.nunit.ExpectedToFail.NestedExceptions.HasInnerException() in FailingTests.cs: line 49 
             */
            [Fact]
            public void HasInnerException()
            {
                try
                {
                    CallSomeMethod();
                }
                catch (Exception ex)
                {
                    throw new InvalidProgramException("Invalid program exception message", ex);
                }
            }

            // TEST: Should fail
            // TEST: Should display "Failed: System.InvalidProgramException: invalid program exception message" in tree view status
            /* TEST: Should display:
System.InvalidProgramException: System.InvalidProgramException : Invalid program exception message
---- System.IO.InvalidDataException : Invalid data exception message
-------- System.InvalidOperationException : Invalid operation exception message

at tests.xunit.ExpectedToFail.NestedExceptions.HasThreeInnerExceptions() in FailingTests.cs: line 94
----- Inner Stack Trace -----
at tests.xunit.ExpectedToFail.NestedExceptions.CallOneMoreMethod() in FailingTests.cs: line 106
at tests.xunit.ExpectedToFail.NestedExceptions.HasThreeInnerExceptions() in FailingTests.cs: line 90
----- Inner Stack Trace -----
at tests.xunit.ExpectedToFail.NestedExceptions.CallAnotherMethod() in FailingTests.cs: line 82
at tests.xunit.ExpectedToFail.NestedExceptions.CallSomeMethod() in FailingTests.cs: line 77
at tests.xunit.ExpectedToFail.NestedExceptions.CallOneMoreMethod() in FailingTests.cs: line 102 
             */
            [Fact]
            public void HasThreeInnerExceptions()
            {
                try
                {
                    CallOneMoreMethod();
                }
                catch (Exception ex)
                {
                    throw new InvalidProgramException("Invalid program exception message", ex);
                }
            }

            private static void CallSomeMethod()
            {
                CallAnotherMethod();
            }

            private static void CallAnotherMethod()
            {
                throw new InvalidOperationException("Invalid operation exception message");
            }

            private static void CallOneMoreMethod()
            {
                try
                {
                    CallSomeMethod();
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException("Invalid data exception message", ex);
                }
            }
        }

        public class FailingTheoryTests
        {
            // TEST: Should run all 3 tests
            // TEST: "failing" should fail, all others should pass
            // BUG: Only reporting one set of results
            [Theory]
            [InlineData("works", 5)]
            [InlineData("failing", 1)]
            [InlineData("works as well", 13)]
            public void OneFailingTheoryTest(string value, int expectedLength)
            {
                Console.WriteLine("OneFailingTheoryTest(" + value + ") with expected length of " + expectedLength);
                Assert.Equal(expectedLength, value.Length);
            }

            // TEST: Should run all 3 tests
            // TEST: "failing" should fail, all others should pass
            // BUG: Only reporting one set of results
            [Theory]
            [InlineData("fails", 6)]
            [InlineData("failing", 1)]
            [InlineData("fail as well", 13)]
            public void MultipleFailingTheoryTest(string value, int expectedLength)
            {
                Console.WriteLine("OneFailingTheoryTest(" + value + ") with expected length of " + expectedLength);
                Assert.Equal(expectedLength, value.Length);
            }
        }

        public class InvalidFactMethod
        {
            // TEST: Method should fail and display exception details in progress tree view area
            // This method is marked as a test because xunit will pick it up and try to run it. Maybe this is wrong...
            [Fact]
            public void TooManyParametersForFact(string shouldNotHaveParameter)
            {
                // Should not get called
            }
        }

        public class CausingFrameworkFailures
        {
            // TEST: Class should fail and display exception (with short name) in progress tree view area
            // Calls ITestRunner:
            public class AmbiguousMethodNames
            {
                // TEST: Test should not run
                [Fact]
                public void MethodsWithOverridesCannotBeUniquelyFoundByName(int arg)
                {
                    // Should not get called
                }

                // TEST: Test should not run
                [Fact]
                public void MethodsWithOverridesCannotBeUniquelyFoundByName(string arg)
                {
                    // Should not get called
                }
            }
        }

        public class FailuresInNonTestMethodCode
        {
            public class ThrowsInConstructor
            {
                public ThrowsInConstructor()
                {
                    throw new InvalidOperationException("Bad things happened in class constructor");
                }

                // TEST: Test fails and displays exception message from constructor in tree view progress area
                // NOTE: Reported to xunit xml logger as "TestFailed"
                [Fact]
                public void ClassThrowsInConstructor()
                {
                    throw new InvalidOperationException("Does not call this method");
                }

                // TEST: Test fails and displays exception message from constructor in tree view progress area
                // NOTE: Reported to xunit xml logger as "TestFailed"
                [Fact]
                public void AnotherTest()
                {
                    throw new InvalidOperationException("Does not call this method");
                }
            }

            public class ThrowsInDispose : IDisposable
            {
                public void Dispose()
                {
                    throw new InvalidOperationException("Something bad happened in class Dispose method");
                }

                // TEST: Test fails and displays exception message in tree view progress area
                // NOTE: Reported to xunit xml logger as "TestFailed"
                [Fact]
                public void WorkingTest()
                {
                    // Do nothing
                }

                // TEST: Test fails and displays exception message in tree view progress area
                // TEST: Test only displays exception from Dispose - it loses the assert exception
                // NOTE: Reported to xunit xml logger as "TestFailed"
                [Fact]
                public void TestThrowsAssert()
                {
                    Assert.Equal(1, 2);
                }
            }

            public class FixtureThatThrowsInConstructor
            {
                public FixtureThatThrowsInConstructor()
                {
                    throw new InvalidOperationException("Something bad happened in the fixture constructor");
                }
            }

            public class FixtureThatThrowsInDispose : IDisposable
            {
                public FixtureThatThrowsInDispose()
                {
                    Value = 134;
                }

                public int Value { get; private set; }

                public void Dispose()
                {
                    throw new InvalidOperationException("Something bad happened in the fixture Dispose method");
                }
            }

            public class ThrowsInFixtureConstructor : IUseFixture<FixtureThatThrowsInConstructor>
            {
                private FixtureThatThrowsInConstructor fixture;

                // TEST: the test should not run
                // TEST: Exception details should be shown in the details pane for the class
                // TEST: Exception message should be shown in the progress report section of the tree view for the class
                // NOTE: Reported to xunit xml logger as "ClassFailed"
                [Fact]
                public void TestShouldNotRunBecauseFixtureConstructorThrows()
                {
                    // Using fixture
                    if (fixture == null) {}
                    throw new InvalidOperationException("Should not get run");
                }

                public void SetFixture(FixtureThatThrowsInConstructor data)
                {
                    fixture = data;
                }
            }

            public class ThrowsInFixtureDispose : IUseFixture<FixtureThatThrowsInDispose>
            {
                private FixtureThatThrowsInDispose fixture;

                // TEST: the test should pass but the class should fail
                // TEST: Exception details should be shown in the details pane for the class
                // TEST: Exception message should be shown in the progress report section of the tree view for the class
                // NOTE: Reported to xunit xml logger as "ClassFailed"
                [Fact]
                public void WorkingTest()
                {
                    Assert.Equal(134, fixture.Value);
                }

                // TEST: Should display "Assert.Equal() failure Expected: 1 Actual 134"
                // TEST: Should display exception message + call stack
                [Fact]
                public void TestThrowsAssert()
                {
                    Assert.Equal(1, fixture.Value);
                }

                public void SetFixture(FixtureThatThrowsInDispose data)
                {
                    fixture = data;
                }
            }

            private class ThrowingBeforeTestAttribute : BeforeAfterTestAttribute
            {
                public override void Before(System.Reflection.MethodInfo methodUnderTest)
                {
                    throw new InvalidOperationException("Message from Before attribute");
                }
            }

            private class ThrowingAfterTestAttribute : BeforeAfterTestAttribute
            {
                public override void After(System.Reflection.MethodInfo methodUnderTest)
                {
                    throw new InvalidOperationException("Message from After attribute");
                }
            }

            private class ThrowingAfterTestAgainAttribute : BeforeAfterTestAttribute
            {
                public override void After(System.Reflection.MethodInfo methodUnderTest)
                {
                    throw new InvalidProgramException("Second thrown exception");
                }
            }

            public class ThrowsInBeforeAfterAttribute
            {
                // TEST: Displays "InvalidOperationException: Message from Before attribute"
                [Fact, ThrowingBeforeTest]
                public void AttributeThrowsBeforeTest()
                {
                    Assert.Equal(1, 1);
                }

                // TEST: Displays "AfterTestException: One or more exceptions were thrown from After methods during test cleanup"
                // TEST: Displays call stack for InvalidOperationException
                [Fact, ThrowingAfterTest]
                public void AttributeThrowsAfterTest()
                {
                    Assert.Equal(1, 1);
                }

                // TEST: Displays "AfterTestException: One or more exceptions were thrown from After methods during test cleanup"
                // TEST: Displays call stack for InvalidOperationException
                // NOTE: Does not display call stack or exception message for exception in Before attribute
                [Fact, ThrowingBeforeTest, ThrowingAfterTest]
                public void AttributesThrowBeforeAndAfterTest()
                {
                    Assert.Equal(1, 1);
                }

                // TEST: Displays "AfterTestException: One or more exceptions were thrown from After methods during test cleanup"
                // TEST: Displays separate call stacks for InvalidOperationException + InvalidProgramException
                // TEST: InvalidOperationException is listed first
                [Fact, ThrowingAfterTest, ThrowingAfterTestAgain]
                public void AttributesThrowMultipleTimesAfterTests()
                {
                    Assert.Equal(1, 1);
                }
            }
        }
    }
}