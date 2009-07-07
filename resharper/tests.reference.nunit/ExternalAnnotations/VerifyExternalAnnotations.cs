using System;
using System.Diagnostics;
using JetBrains.Annotations;
using NUnit.Framework;

namespace tests.reference.nunit.ExternalAnnotations
{
    [TestFixture]
    public class VerifyExternalAnnotations
    {
        // I don't really want to run the tests, but I don't want them
        // flagged as unused by SWA, either
        [Test]
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

            Assert.IsNotNull(nullStringValue);

            // TEST: nullStringValue is not flagged as "Possible System.NullReferenceException"
            MethodWithNotNullParameter(nullStringValue);
        }

        private static void AssertIsNullProbablyDoesSomethingUsefulButIDontKnowWhat()
        {
            object o = new FlagsAttribute();
            var o2 = o as AttributeUsageAttribute;

            Assert.IsNull(o2);

            // TEST: I can't figure out a really sensible test for Assert.IsNull, but this seems to do 
            // something. If Assert.IsNull has the attribute, o2 == null below *IS* flagged as "Expression
            // is always true"
            if(o2 == null)
                return;
        }

        private static void AssertIsTruePreventsAlwaysFalseExpressionBeingFlagged()
        {
            const bool myFlag = false;

            Assert.IsTrue(myFlag);

            // TEST: This condition should not be flagged as "Expression is always true"
            while (!myFlag)
            {
                break;
            }
        }

        private static void AssertIsFalsePreventsAlwaysTrueExpressionBeingFlagged()
        {
            const bool myFlag = true;

            Assert.IsFalse(myFlag);

            // TEST: This condition should not be flagged as "Expression is always true"
            while (myFlag)
            {
                break;
            }
        }
    }
}