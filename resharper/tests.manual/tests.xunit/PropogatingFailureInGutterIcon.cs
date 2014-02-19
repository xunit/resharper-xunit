using System;
using Xunit;
using Xunit.Extensions;

namespace tests.xunit.eyeball
{
    namespace PropogatingFailureInGutterIcon
    {
        // TEST: Class should be marked as failing
        public class TestWithFailingMethod
        {
            // TEST: Method should be marked as failing
            [Fact]
            public void MethodFails()
            {
                throw new Exception();
            }

            // TEST: Method should be marked as passing
            [Fact]
            public void MethodPasses()
            {
            }
        }

        // TEST: Class should be marked as failing
        public class TestWithFailingTheory
        {

            // TEST: Method should be marked as passing
            [Theory]
            [InlineData(12)]
            [InlineData(23)]
            public void TheoryPasses2(int value)
            {
            }

            // TEST: Method should be marked as passing
            [Theory]
            [InlineData(12)]
            [InlineData(23)]
            public void TheoryPass(int value)
            {
            }

            // TEST: Method should be marked as failing
            [Theory]
            [InlineData(12)]
            [InlineData(23)]
            public void TheoryFails(int value)
            {
                if (value == 23)
                    throw new Exception();
            }

            // TEST: Method should be marked as passing
            [Theory]
            [InlineData(12)]
            [InlineData(23)]
            public void TheoryPasses(int value)
            {
            }
        }

        // TEST: Class should be marked as skipped
        public class TestWithSkippedMethods
        {
            // TEST: Method should be marked as skipped
            [Fact(Skip = "Skipped")]
            public void MethodSkipped()
            {
            }
        }

        // TEST: Class should be marked as skipped before running
        // TEST: Class should be marked as failing after run
        public class TestWithSkippedAndFailingMethods
        {
            // TEST: Method should be marked as failing
            [Fact]
            public void MethodFailed()
            {
                throw new Exception();
            }

            // TEST: Method should be marked as skipped
            [Fact(Skip = "Skipped")]
            public void MethodSkipped()
            {
            }
        }
    }
}