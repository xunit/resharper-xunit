using System;
using JetBrains.Annotations;
using Xunit;

namespace tests.xunit.eyeball.sourcecode
{
    public class VerifyExternalAnnotations
    {
        [Fact]
        public void ThisTestIsNotRunButIsUsedToMarkAllOtherMethodsAsInUse()
        {
            try
            {
                AssertIsNotNull_RemovesNullReferenceWarning_PassingVariableToNotNullMethod();
                AssertIsNotNull_RemovesNullReferenceWarning_AccessingPropertiesOnVariable();
                AssertIsNotNull_RemovesNullReferenceWarning_FromValueFlaggedWithCanBeNull();

                AssertIsNull_AddsNullReferenceWarning();

                AssertIsTrue_AddsExpressionAlwaysTrueWarning();

                AssertIsFalse_AddsExpressionAlwaysFalseWarning();
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
            Console.WriteLine(stringCannotBeNull.ToLower());
        }

        private static void MethodWithoutNotNullParameter(string stringValue)
        {
            Console.WriteLine(stringValue.ToLower());
        }

        [CanBeNull]
        private static string MethodWithCanBeNullReturnValue()
        {
            return null;
        }

        private static void AssertIsNotNull_RemovesNullReferenceWarning_PassingVariableToNotNullMethod()
        {
            string nullStringValue = null;
            string uncheckedNullStringValue = null;

            Assert.NotNull(nullStringValue);

            // At this point, resharper knows that nullStringValue is NOT null (we'll have thrown an exception if it IS null)
            // IsNotNull doesn't have a [NotNull] attribute because IsNotNull would have a "Check for null" warning
            // which is kind of redundant for an assert

            // TEST: nullStringValue is not flagged
            MethodWithNotNullParameter(nullStringValue);

            // CONTROL: uncheckedNullStringValue should be flagged as "Expression is always null"
            MethodWithoutNotNullParameter(uncheckedNullStringValue);
        }

        private static void AssertIsNotNull_RemovesNullReferenceWarning_AccessingPropertiesOnVariable()
        {
            string nullStringValue = null;
            string uncheckedNullStringValue = null;

            Assert.NotNull(nullStringValue);

            // At this point, resharper knows that nullStringValue is NOT null (we'll have thrown an exception if it IS null)
            // IsNotNull doesn't have a [NotNull] attribute because IsNotNull would have a "Check for null" warning
            // which is kind of redundant for an assert

            // TEST: nullStringValue is not flagged
            Console.WriteLine(nullStringValue.Length);

            // CONTROL: uncheckedNullStringValue should be flagged as "Possible System.NullReferenceException"
            Console.WriteLine(uncheckedNullStringValue.Length);
        }

        private static void AssertIsNotNull_RemovesNullReferenceWarning_FromValueFlaggedWithCanBeNull()
        {
            var canBeNullValue = MethodWithCanBeNullReturnValue();
            var uncheckedCanBeNullValue = MethodWithCanBeNullReturnValue();

            Assert.NotNull(canBeNullValue);

            // At this point, resharper knows that nullStringValue is NOT null (we'll have thrown an exception if it IS null)
            // IsNotNull doesn't have a [NotNull] attribute because IsNotNull would have a "Check for null" warning
            // which is kind of redundant for an assert

            // TEST: canBeNullValue should NOT be flagged
            Console.WriteLine(canBeNullValue.Length);

            // CONTROL: uncheckedCanBeNullValue should be flagged as "Possible System.NullReferenceException"
            Console.WriteLine(uncheckedCanBeNullValue.Length);
        }

        private static void AssertIsNull_AddsNullReferenceWarning()
        {
            var canBeNullValue = MethodWithCanBeNullReturnValue();
            var uncheckedCanBeNullValue = MethodWithCanBeNullReturnValue();

            Assert.Null(canBeNullValue);

            // At this point, we've told resharper that canBeNullValue IS null (we'll have thrown an exception if it's NOT)

            // TEST: (canBeNullValue == null) is flagged as "Expression is always true"
            if (canBeNullValue == null)
                Console.WriteLine("Is null");

            // CONTROL: (uncheckedCanBeNullValue == null) is not flagged
            if (uncheckedCanBeNullValue == null)
                Console.WriteLine("Is null");
        }

        private static bool MethodReturnsBool(bool value)
        {
            return value;
        }

        private static void AssertIsTrue_AddsExpressionAlwaysTrueWarning()
        {
            var trueValue = MethodReturnsBool(true);
            var uncheckedTrueValue = MethodReturnsBool(true);

            Assert.True(trueValue);

            // At this point, resharper knows that trueValue is always true

            // TEST: trueValue is flagged as "Expression is always true"
            Console.WriteLine(trueValue ? "true" : "false");

            // CONTROL: uncheckedTrueValue is NOT flagged
            Console.WriteLine(uncheckedTrueValue ? "true" : "false");
        }

        private static void AssertIsFalse_AddsExpressionAlwaysFalseWarning()
        {
            var falseValue = MethodReturnsBool(false);
            var uncheckedFalseValue = MethodReturnsBool(false);

            Assert.False(falseValue);

            // At this point, resharper knows that falseValue is always false

            // TEST: trueValue is flagged as "Expression is always false"
            Console.WriteLine(falseValue ? "true" : "false");

            // CONTROL: uncheckedTrueValue is NOT flagged
            Console.WriteLine(uncheckedFalseValue ? "true" : "false");
        }
    }
}