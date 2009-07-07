using System;
using JetBrains.Annotations;
using Xunit;

namespace tests.xunit.ExternalAnnotations
{
    public class VerifyExternalAnnotations
    {
        // I don't really want to run the tests, but I don't want them
        // flagged as unused by SWA, either
        [Fact]
        public void KeepSolutionWideAnalysisHappy()
        {
            try
            {
                AssertIsNotNullRemovesNullReferenceWarning();
                AssertIsNullProbablyDoesSomethingUsefulButIDontKnowWhat();
                AssertIsTruePreventsAlwaysFalseExpressionBeingFlagged();
                AssertIsFalsePreventsAlwaysTrueExpressionBeingFlagged();
            }
            catch
            {
                return;
            }
        }

        // Marked with the same attribute that JetBrains have used for the rest of the framework
        // If we want to try one of their marked classes, try System.Activator.CreateInstance((Type)null)
        private static void MethodWithNotNullParameter([NotNull] string stringCannotBeNull)
        {
            stringCannotBeNull.ToLower();
        }

        private static void AssertIsNotNullRemovesNullReferenceWarning()
        {
            string nullStringValue = null;

            Assert.NotNull(nullStringValue);

            // TEST: nullStringValue is not flagged as "Possible System.NullReferenceException"
            MethodWithNotNullParameter(nullStringValue);
        }

        private static void AssertIsNullProbablyDoesSomethingUsefulButIDontKnowWhat()
        {
            object o = new FlagsAttribute();
            var o2 = o as AttributeUsageAttribute;

            Assert.Null(o2);

            // TEST: I can't figure out a really sensible test for Assert.IsNull, but this seems to do 
            // something. If Assert.IsNull has the attribute, o2 == null below *IS* flagged as "Expression
            // is always true"
            if (o2 == null)
                return;
        }

        private static void AssertIsTruePreventsAlwaysFalseExpressionBeingFlagged()
        {
            const bool myFlag = false;

            Assert.True(myFlag);

            // TEST: This condition should not be flagged as "Expression is always true"
            while (!myFlag)
            {
                break;
            }
        }

        private static void AssertIsFalsePreventsAlwaysTrueExpressionBeingFlagged()
        {
            const bool myFlag = true;

            Assert.False(myFlag);

            // TEST: This condition should not be flagged as "Expression is always true"
            while (myFlag)
            {
                break;
            }
        }
    }
}