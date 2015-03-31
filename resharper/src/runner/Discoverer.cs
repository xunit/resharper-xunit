using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using XunitContrib.Runner.ReSharper.RemoteRunner.Logging;
using XunitContrib.Runner.ReSharper.RemoteRunner.Tasks;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public class Discoverer
    {
        private readonly ITestFrameworkDiscoverer discoverer;
        private readonly RunContext runContext;

        public Discoverer(ITestFrameworkDiscoverer discoverer, RunContext runContext)
        {
            this.discoverer = discoverer;
            this.runContext = runContext;
        }

        public IList<ITestCase> GetTestCases()
        {
            var visitor = new TestDiscoveryVisitor(runContext);

            // TODO: Proper options
            var options = TestFrameworkOptions.ForDiscovery();

            Logger.LogVerbose("Starting discovery");
            Logger.LogVerbose("  Pre-enumerating theories: {0}", options.GetPreEnumerateTheoriesOrDefault());

            discoverer.Find(false, visitor, options);
            visitor.Finished.WaitOne();

            Logger.LogVerbose("Filtering test cases");
            return visitor.TestCases;
        }

        private class TestDiscoveryVisitor : TestMessageVisitor<IDiscoveryCompleteMessage>
        {
            private readonly RunContext runContext;

            public TestDiscoveryVisitor(RunContext runContext)
            {
                this.runContext = runContext;
                TestCases = new List<ITestCase>();
            }

            public List<ITestCase> TestCases { get; private set; }

            protected override bool Visit(ITestCaseDiscoveryMessage discovery)
            {
                Logger.LogVerbose("Discovered: {0}", discovery.TestCase.Format());
                if (ShouldRunTestCase(discovery.TestCase))
                    TestCases.Add(discovery.TestCase);
                return true;
            }

            // TODO: Make all of this nicer
            // I don't think it can be nicer until the discovery is in the editor, in which
            // case, discoverer here disappears - we'll just run the serialised TestCases
            private bool ShouldRunTestCase(ITestCase testCase)
            {
                var isRequestedMethod = IsRequestedMethod(testCase);
                var isDynamicMethod = IsDynamicMethod(testCase);

                var shouldRun = isRequestedMethod || isDynamicMethod;

                Logger.LogVerbose(" {0} test case {1}", shouldRun ? "Including" : "Excluding", testCase.Format());

                return shouldRun;
            }

            // Is this TestCase a method or theory that has been asked for from the main process?
            // That is, is it a known element that is already part of the current session?
            private bool IsRequestedMethod(ITestCase testCase)
            {
                var typeName = testCase.TestMethod.TestClass.Class.Name;
                var methodName = testCase.TestMethod.Method.Name;
                var displayName = testCase.DisplayName ?? MakeFallbackDisplayName(typeName, methodName);

                if (IsTheory(displayName, typeName, methodName))
                {
                    var hasTheoryTask = runContext.HasTheoryTask(displayName, typeName, methodName);
                    Logger.LogVerbose(" Theory test case is {0}a requested theory: {1}",
                        hasTheoryTask ? string.Empty : "NOT ",
                        testCase.Format());
                    return hasTheoryTask;
                }

                var hasMethodTask = runContext.HasMethodTask(typeName, methodName);
                Logger.LogVerbose(" Test case is {0}a requested method: {1}", hasMethodTask ? string.Empty : "NOT ",
                    testCase.Format());
                return hasMethodTask;
            }

            // Is this TestCase a dynamic element? One that is created at runtime, and therefore not
            // included in the current session. We do this by looking to see if it's a theory, which
            // is most frequently a dynamic element, or if it's a method belonging to a class, but
            // not known to that class (each class task keeps a track of all discovered test methods,
            // so we can tell if it's something we didn't know about, or something we didn't want to
            // run)
            private bool IsDynamicMethod(ITestCase testCase)
            {
                var typeName = testCase.TestMethod.TestClass.Class.Name;
                var methodName = testCase.TestMethod.Method.Name;
                var displayName = testCase.DisplayName ?? MakeFallbackDisplayName(typeName, methodName);

                var classTaskWrapper = runContext.GetRemoteTask(typeName);
                if (classTaskWrapper == null)
                {
                    Logger.LogVerbose(
                        " Test case class unknown. Cannot be a dynamic test of a requested class. {0} - {1}",
                        testCase.TestMethod.TestClass.Class.Name, testCase.Format());
                    return false;
                }

                var classTask = (XunitTestClassTask) classTaskWrapper.RemoteTask;

                if (IsTheory(displayName, typeName, methodName))
                {
                    var isDynamicTheory = !classTask.IsKnownMethod(displayName.Replace(typeName + ".", string.Empty));
                    Logger.LogVerbose(" Theory test case is {0}a requested theory: {1}",
                        isDynamicTheory ? string.Empty : "NOT ",
                        testCase.Format());
                    return isDynamicTheory;
                }

                var isDynamicMethod = !classTask.IsKnownMethod(methodName);
                Logger.LogVerbose(" Test case is {0}a dynamic method: {1}", isDynamicMethod ? string.Empty : "NOT ",
                    testCase.Format());
                return isDynamicMethod;
            }

            private static bool IsTheory(string name, string type, string method)
            {
                return name != type + "." + method;
            }

            private static string MakeFallbackDisplayName(string typeName, string methodName)
            {
                return typeName + "." + methodName;
            }
        }
    }
}