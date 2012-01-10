using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;

namespace tests.reference.nunit
{
    namespace ExpectedToFail
    {
        [TestFixture]
        public class FailingTests
        {
            [Test]
            public void FailsDueToThrownException()
            {
                throw new Exception("this test should fail");
            }

            [Test]
            public void FailsDueToAssert()
            {
                Assert.AreEqual("this test", "should fail");
            }

            [Test, Ignore("nunit doesn't capture Debug.Assert")]
            public void FailsDueToDebugAssert()
            {
                Debug.Assert(1 == 0, "message - supposed to fail", "detailed message - supposed to fail");
            }

            [Test, Ignore("nunit doesn't capture Trace.Assert")]
            public void FailsDueToTraceAssert()
            {
                Trace.Assert(1 == 0, "message - supposed to fail", "detailed message - supposed to fail");
            }
        }

        [TestFixture]
        public class NestedExceptions
        {
            [Test]
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

            [Test]
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

        [TestFixture]
        public class ThrowsInConstructor
        {
            public ThrowsInConstructor()
            {
                throw new InvalidOperationException("Something bad happened in the constructor");
            }

            [Test]
            public void TestWillNeverGetExecuted()
            {
                // Never gets run...
            }
        }

        [TestFixture]
        public class ThrowsInSetUp
        {
            [SetUp]
            public void SetUp()
            {
                throw new InvalidOperationException("Something bad happened in SetUp");
            }

            // The test fails and displays the exception message in the progress tree view pane
            [Test]
            public void TestFailsBeforeItCanRun()
            {
                // Do nothing
            }
        }

        [TestFixture]
        public class ThrowsInTearDown
        {
            [TearDown]
            public void TearDown()
            {
                throw new InvalidOperationException("Something bad happened in TearDown");
            }

            // The test fails and displays the exception message in the progress tree view pane
            [Test]
            public void TestFailsAfterItRuns()
            {
                // Do nothing
            }

            // The test fails with multiple exceptions - the assert exception looks like an inner exception
            [Test]
            public void TestFailsAfterCausingAssert()
            {
                Assert.Fail("Testing assert failures + throwing exceptions");
            }
        }

        [TestFixture]
        public class ThrowsInTestFixtureSetUp
        {
            [TestFixtureSetUp]
            public void TestFixtureSetUp()
            {
                throw new InvalidOperationException("Something bad happened in the TestFixtureSetUp method");
            }

            [Test]
            public void TestWillNeverGetExecuted()
            {
                // Do nothing
            }
        }

        [TestFixture]
        public class ThrowsInTestFixtureTearDown
        {
            [TestFixtureTearDown]
            public void TestFixtureTearDown()
            {
                throw new InvalidOperationException("Something bad happened in the TestFixtureTearDown method");
            }

            [Test]
            public void WorkingTest()
            {
                // Do nothing
            }

            [Test]
            public void TestThrowsAssert()
            {
                Assert.Fail("Failing test");
            }
        }
    }
}