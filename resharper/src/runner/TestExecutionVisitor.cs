using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using XunitContrib.Runner.ReSharper.RemoteRunner.Logging;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public class TestExecutionVisitor : TestMessageVisitor<ITestAssemblyFinished>
    {
        private readonly RunContext context;

        public TestExecutionVisitor(RunContext context)
        {
            this.context = context;
        }

        public override bool OnMessage(IMessageSinkMessage message)
        {
            if (Logger.IsEnabled)
            {
                var identifier = MessageLogFormatter.GetIdentifier(message);
                Logger.LogVerbose("  xunit message: {0} for {1}", message, identifier);
            }
            return base.OnMessage(message);
        }

        protected override bool Visit(ITestClassStarting testClassStarting)
        {
            var taskInfo = context.GetRemoteTask(testClassStarting);
            taskInfo.Starting();
            return context.ShouldContinue;
        }

        protected override bool Visit(ITestMethodStarting testMethodStarting)
        {
            var taskInfo = context.GetRemoteTask(testMethodStarting);
            taskInfo.Starting();
            return context.ShouldContinue;
        }

        // There can be one or more test cases per method
        // Pre-enumerated theories are individual test cases
        // A test case will map to either a method element, or a theory element
        protected override bool Visit(ITestCaseStarting testCaseStarting)
        {
            var taskInfo = context.GetRemoteTask(testCaseStarting);
            taskInfo.Starting();
            return context.ShouldContinue;
        }

        // There can be one or more test per test case
        // Late-enumerated theories have one test case and many tests
        // A test will map to its parent test case's element (i.e. method or theory)
        protected override bool Visit(ITestStarting testStarting)
        {
            var taskInfo = context.GetRemoteTask(testStarting);
            taskInfo.Starting();
            return context.ShouldContinue;
        }

        protected override bool Visit(ITestOutput testOutput)
        {
            var task = context.GetRemoteTask(testOutput);
            task.Output(testOutput.Output);
            return context.ShouldContinue;
        }

        protected override bool Visit(ITestSkipped testSkipped)
        {
            var task = context.GetRemoteTask(testSkipped);
            task.Skipped(testSkipped.Reason);
            return context.ShouldContinue;
        }

        protected override bool Visit(ITestPassed testPassed)
        {
            var task = context.GetRemoteTask(testPassed);
            task.Passed();
            return context.ShouldContinue;
        }

        protected override bool Visit(ITestFailed testFailed)
        {
            var task = context.GetRemoteTask(testFailed);
            string message;
            var exceptions = testFailed.ConvertExceptions(out message);
            task.Failed(exceptions, message);
            return context.ShouldContinue;
        }

        protected override bool Visit(ITestFinished testFinished)
        {
            var task = context.GetRemoteTask(testFinished);
            task.Finished(testFinished.ExecutionTime);
            return context.ShouldContinue;
        }

        protected override bool Visit(ITestCaseFinished testCaseFinished)
        {
            var task = context.GetRemoteTask(testCaseFinished);
            task.Finished(testCaseFinished.ExecutionTime, testCaseFinished.TestsFailed > 0);
            return context.ShouldContinue;
        }

        protected override bool Visit(ITestMethodFinished testMethodFinished)
        {
            var task = context.GetRemoteTask(testMethodFinished);
            task.Finished(testMethodFinished.ExecutionTime, testMethodFinished.TestsFailed > 0);
            return context.ShouldContinue;
        }

        protected override bool Visit(ITestClassFinished testClassFinished)
        {
            var task = context.GetRemoteTask(testClassFinished);
            task.Finished(testClassFinished.ExecutionTime, testClassFinished.TestsFailed > 0);
            return context.ShouldContinue;
        }

        // Called for xunit1:
        // 1. Catastrophic error, i.e. the test runner throws. The only expected exception
        //    is for ambiguous method names. Test run is aborted
        // 2. class failure, i.e. fixture's ctor or dispose throws. Test methods are not run
        //    if the fixture's ctor throws. Not called for exceptions in class ctor/dispose
        //    (these are reported as failing test methods, as the class is created for each
        //     test method)
        // Not called for xunit2
        protected override bool Visit(IErrorMessage errorMessage)
        {
            // TODO: Verify what error.TestCases is in both scenarios above
            var classNames = string.Join(", ",
                errorMessage.TestCases.Select(tc => tc.TestMethod.TestClass.Class.Name).Distinct().ToArray());
            var methodMessage = string.Format("Class failed in {0}", classNames);
            HandleFailure(errorMessage, errorMessage.TestCases, methodMessage);
            return context.ShouldContinue;
        }

        protected override bool Visit(ITestCollectionCleanupFailure testCollectionCleanupFailure)
        {
            // TODO: What causes this to be called?
            // TODO: Add a collection node?
            var methodMessage = string.Format("Collection cleanup failed in {0}",
                testCollectionCleanupFailure.TestCollection.DisplayName);
            HandleFailure(testCollectionCleanupFailure, testCollectionCleanupFailure.TestCases, methodMessage);
            return context.ShouldContinue;
        }

        protected override bool Visit(ITestClassCleanupFailure testClassCleanupFailure)
        {
            var methodMessage = string.Format("Class cleanup failed in {0}",
                testClassCleanupFailure.TestClass.Class.Name);
            HandleFailure(testClassCleanupFailure, testClassCleanupFailure.TestCases, methodMessage);
            return context.ShouldContinue;
        }

        protected override bool Visit(ITestMethodCleanupFailure testMethodCleanupFailure)
        {
            // TODO: What causes this to be called? Not captured in existing tests
            // BeforeAfterAttribute?
            return context.ShouldContinue;
        }

        protected override bool Visit(ITestCaseCleanupFailure testCaseCleanupFailure)
        {
            // TODO: What causes this to be called? Not captured in existing tests
            // BeforeAfterAttribute?
            return context.ShouldContinue;
        }

        protected override bool Visit(ITestCleanupFailure testCleanupFailure)
        {
            // TODO: What causes this to be called? Not captured in existing tests
            // BeforeAfterAttribute?
            return context.ShouldContinue;
        }

        private void HandleFailure(IFailureInformation failureInformation, IEnumerable<ITestCase> testCases, string methodMessage)
        {
            string classMessage;
            var classExceptions = failureInformation.ConvertExceptions(out classMessage);

            var testClasses = from testCase in testCases
                              select testCase.TestMethod.TestClass.Class.Name;
            foreach (var typeName in testClasses.Distinct())
            {
                var classTask = context.GetRemoteTask(typeName);
                classTask.Failed(classExceptions, classMessage, methodMessage);

                // We shouldn't need to call Finished, as xunit2 will call Finished for us
                // if the failure happens during class execution. However, it doesn't call
                // Finished when xunit1 has an ambiguous method exception, and when an
                // exception happens in test collection cleanup (the class is already
                // finished at that point. This would go away if we support collections
                // as a node in the tree)
                classTask.Finished();
            }
        }
    }
}