using System;
using Xunit;

namespace tests.xunit
{
    namespace InvalidClassVisibility
    {
        // TEST: Solution Wide Analysis should flag this class as unused
        class PrivateClassIsNotTestClass
        {
            // TEST: This method should not be flagged or run
            // TEST: Solution Wide Analysis should flag this method as unused
            [Fact]
            public void PublicTestMethodOnPrivateClassIsNotValid()
            {
                throw new InvalidOperationException("Should not be run as a test method");
            }

            // TEST: This method should not be flagged or run
            // TEST: Solution Wide Analysis should flag this method as unused
            [Fact]
            internal void InternalTestMethodIsNotValid()
            {
                throw new InvalidOperationException("Should not be run as a test method");
            }

            // TEST: This method should not be flagged or run
            // TEST: Solution Wide Analysis should flag this method as unused
            [Fact]
            protected void ProtectedTestMethodIsNotValid()
            {
                throw new InvalidOperationException("Should not be run as a test method");
            }

            // TEST: This method should not be flagged or run
            // TEST: ReSharper (not SWA) should mark method as unused
            [Fact]
            private void PrivateTestMethodIsNotValid()
            {
                throw new InvalidOperationException("Should not be run as a test method");
            }

            public void ExampleUnusedPublicMethod()
            {
            }

            private void ExampleUnusedPrivateMethod()
            {
            }
        }

        // TEST: Solution Wide Analysis should flag this class as unused
        internal class InternalClassIsNotTestClass
        {
            // TEST: This method should not be flagged or run
            // TEST: Solution Wide Analysis should flag this method as unused
            [Fact]
            public void PublicTestMethodOnPrivateClassIsNotValid()
            {
                throw new InvalidOperationException("Should not be run as a test method");
            }

            public void ExampleUnusedPublicMethod()
            {
            }

            // TEST: This method should not be flagged or run
            // TEST: Solution Wide Analysis should flag this method as unused
            [Fact]
            internal void InternalTestMethodIsNotValid()
            {
                throw new InvalidOperationException("Should not be run as a test method");
            }

            // TEST: This method should not be flagged or run
            // TEST: Solution Wide Analysis should flag this method as unused
            [Fact]
            protected void ProtectedTestMethodIsNotValid()
            {
                throw new InvalidOperationException("Should not be run as a test method");
            }

            // TEST: This method should not be flagged or run
            // TEST: ReSharper (not SWA) should mark method as unused
            [Fact]
            private void PrivateTestMethodIsNotValid()
            {
                throw new InvalidOperationException("Should not be run as a test method");
            }

            private void ExampleUnusedPrivateMethod()
            {
            }
        }
    }
}