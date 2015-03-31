using System.Collections.Generic;
using System.Linq;
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
            var visitor = new TestDiscoveryVisitor();

            // TODO: Proper options
            var options = TestFrameworkOptions.ForDiscovery();

            Logger.LogVerbose("Starting discovery");
            Logger.LogVerbose("  Pre-enumerating theories: {0}", options.GetPreEnumerateTheoriesOrDefault());

            discoverer.Find(false, visitor, options);
            visitor.Finished.WaitOne();

            Logger.LogVerbose("Filtering test cases");
            return visitor.TestCases.Where(ShouldRunTestCase).ToList();
        }

        private bool ShouldRunTestCase(ITestCase testCase)
        {
            // TODO: These methods are pretty nasty
            var isRequestedMethod = IsRequestedMethod(testCase);
            var isDynamicMethod = IsDynamicMethod(testCase, runContext);

            var shouldRun = isRequestedMethod || isDynamicMethod;

            Logger.LogVerbose(" {0} test case {1}", shouldRun ? "Including" : "Excluding", testCase.Format());

            return shouldRun;
        }

        private bool IsRequestedMethod(ITestCase testCase)
        {
            var typeName = testCase.TestMethod.TestClass.Class.Name;
            var methodName = testCase.TestMethod.Method.Name;
            var displayName = testCase.DisplayName ?? MakeDisplayName(typeName, methodName);

            if (IsTheory(displayName, typeName, methodName))
            {
                var hasTheoryTask = runContext.HasTheoryTask(displayName, typeName, methodName);
                Logger.LogVerbose(" Theory test case is {0}a requested theory: {1}", hasTheoryTask ? string.Empty : "NOT ",
                    testCase.Format());
                return hasTheoryTask;
            }

            var hasMethodTask = runContext.HasMethodTask(typeName, methodName);
            Logger.LogVerbose(" Test case is {0}a requested method: {1}", hasMethodTask ? string.Empty : "NOT ",
                testCase.Format());
            return hasMethodTask;
        }

        private static bool IsDynamicMethod(ITestCase testCase, RunContext runContext)
        {
            var typeName = testCase.TestMethod.TestClass.Class.Name;
            var methodName = testCase.TestMethod.Method.Name;
            var displayName = testCase.DisplayName ?? MakeDisplayName(typeName, methodName);

            var classTaskWrapper = runContext.GetRemoteTask(typeName);
            if (classTaskWrapper == null)
            {
                Logger.LogVerbose(" Test case class unknown. Cannot be a dynamic test of a requested class. {0} - {1}",
                    testCase.TestMethod.TestClass.Class.Name, testCase.Format());
                return false;
            }

            var classTask = (XunitTestClassTask)classTaskWrapper.RemoteTask;

            if (IsTheory(displayName, typeName, methodName))
            {
                var isDynamicTheory = !classTask.IsKnownMethod(displayName.Replace(typeName + ".", string.Empty));
                Logger.LogVerbose(" Theory test case is {0}a requested theory: {1}", isDynamicTheory ? string.Empty : "NOT ",
                    testCase.Format());
                return isDynamicTheory;
            }

            var isDynamicMethod = !classTask.IsKnownMethod(methodName);
            Logger.LogVerbose(" Test case is {0}a dynamic method: {1}", isDynamicMethod ? string.Empty : "NOT ", testCase.Format());
            return isDynamicMethod;
        }

        private static bool IsTheory(string name, string type, string method)
        {
            return name != type + "." + method;
        }

        private static string MakeDisplayName(string typeName, string methodName)
        {
            return typeName + "." + methodName;
        }

    }
}