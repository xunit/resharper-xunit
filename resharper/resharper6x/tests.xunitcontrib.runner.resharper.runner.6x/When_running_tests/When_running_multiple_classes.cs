using System;
using Xunit;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class When_running_multiple_classes
    {
        [Fact]
        public void Should_notify_starting_and_finishing_each_class()
        {
            var testRun = new TestRun();
            var testClass1 = testRun.AddClass("TestNamespace.TestClass1");
            testClass1.AddPassingTest("TestMethod");
            var testClass2 = testRun.AddClass("TestNamespace.TestClass2");
            testClass2.AddPassingTest("OtherTestMethod");

            testRun.Run();

            var messages = testRun.Messages.AssertContainsTaskStarting(testClass1.ClassTask);
            messages = messages.AssertContainsSuccessfulTaskFinished(testClass1.ClassTask);
            messages = messages.AssertContainsTaskStarting(testClass2.ClassTask);
            messages.AssertContainsSuccessfulTaskFinished(testClass2.ClassTask);
        }

        [Fact]
        public void Should_continue_running_classes_after_failing_tests()
        {
            var testRun = new TestRun();
            var testClass1 = testRun.AddClass("TestNamespace.TestClass1");
            testClass1.AddFailingTest("TestMethod", new EqualException(23, 186));
            var testClass2 = testRun.AddClass("TestNamespace.TestClass2");
            testClass2.AddPassingTest("OtherTestMethod");

            testRun.Run();

            var messages = testRun.Messages.AssertContainsTaskStarting(testClass1.ClassTask);
            messages = messages.AssertContainsSuccessfulTaskFinished(testClass1.ClassTask);
            messages = messages.AssertContainsTaskStarting(testClass2.ClassTask);
            messages.AssertContainsSuccessfulTaskFinished(testClass2.ClassTask);
        }

        [Fact]
        public void Should_continue_running_classes_after_test_class_failure()
        {
            var testRun = new TestRun();
            var testClass1 = testRun.AddClass("TestNamespace.TestClass1");
            testClass1.AddMethod("TestMethod");
            var classException = new ArgumentException("Ambiguous method named TestMethod1 in type " + testClass1.ClassTask.TypeName);
            testClass1.InfrastructureException = classException;

            var testClass2 = testRun.AddClass("TestNamespace.TestClass2");
            testClass2.AddPassingTest("OtherTestMethod");

            testRun.Run();

            var messages = testRun.Messages.AssertContainsTaskStarting(testClass1.ClassTask);
            messages = messages.AssertContainsFailedInfrastructureTaskFinished(testClass1.ClassTask, classException);
            messages = messages.AssertContainsTaskStarting(testClass2.ClassTask);
            messages.AssertContainsSuccessfulTaskFinished(testClass2.ClassTask);
        }
    }
}