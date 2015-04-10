using System.Collections.Generic;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit.Abstractions;
using XunitContrib.Runner.ReSharper.RemoteRunner.Tasks;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    // TODO: Review this. It's getting very messy
    // The lock is far too coarse, and could probably do with being a ReaderWriterLockSlim
    // Perhaps split the different maps into objects so it's easier to reason about the
    // locking requirements?
    public class RunContext
    {
        private readonly IRemoteTaskServer server;
        private readonly object lockObject = new object();

        // TODO: That's a lot of caches...
        private readonly Dictionary<string, RemoteTaskWrapper> tasksByClassName = new Dictionary<string, RemoteTaskWrapper>();
        private readonly Dictionary<string, RemoteTaskWrapper> tasksByFullyQualifiedMethodName = new Dictionary<string, RemoteTaskWrapper>();
        private readonly Dictionary<string, RemoteTaskWrapper> tasksByTestCaseUniqueId = new Dictionary<string, RemoteTaskWrapper>();
        private readonly Dictionary<string, RemoteTaskWrapper> tasksByTheoryId = new Dictionary<string, RemoteTaskWrapper>();
        private readonly Dictionary<ITest, RemoteTaskWrapper> tasksByTestInstance = new Dictionary<ITest, RemoteTaskWrapper>();

        private readonly HashSet<RemoteTask> handledDynamicTasks = new HashSet<RemoteTask>();

        public RunContext(IRemoteTaskServer server)
        {
            this.server = server;

            ShouldContinue = true;
        }

        public void Abort()
        {
            ShouldContinue = false;
        }

        public bool ShouldContinue { get; private set; }

        public void Add(XunitTestClassTask classTask)
        {
            var key = classTask.TypeName;

            lock (lockObject)
            {
                if (!tasksByClassName.ContainsKey(key))
                    tasksByClassName.Add(key, new RemoteTaskWrapper(classTask, server));
            }
        }

        public void Add(XunitTestMethodTask methodTask)
        {
            var key = string.Format("{0}.{1}", methodTask.TypeName, methodTask.MethodName);

            lock (lockObject)
            {
                if (!tasksByFullyQualifiedMethodName.ContainsKey(key))
                    AddMethodTask(key, methodTask.TypeName, new RemoteTaskWrapper(methodTask, server));
            }
        }

        public void Add(XunitTestTheoryTask theoryTask)
        {
            // TheoryName is the display name with any type prefix stripped off
            // TODO: Why strip off the type prefix? It's not used anywhere
            var fullyQualifiedMethodName = string.Format("{0}.{1}", theoryTask.TypeName, theoryTask.MethodName);
            var key = string.Format("{0}-{1}", fullyQualifiedMethodName, theoryTask.TheoryName);

            lock (lockObject)
            {
                // TODO: Does this handle repeated tasks?
                if (!tasksByTheoryId.ContainsKey(key))
                    AddTheoryTask(key, fullyQualifiedMethodName, new RemoteTaskWrapper(theoryTask, server));
            }
        }

        public void AddRange(IEnumerable<ITestCase> testCases)
        {
            foreach (var testCase in testCases)
                Add(testCase);
        }

        public void Add(ITestCase testCase)
        {
            lock (lockObject)
            {
                if (tasksByTestCaseUniqueId.ContainsKey(testCase.UniqueID))
                    return;

                // A TestCase might be a single method, or a pre-enumerated theory
                ITestMethod testMethod = testCase.TestMethod;
                var task = IsTheory(testCase) ? GetTheoryTask(testCase.TestMethod, testCase.DisplayName) : GetMethodTask(testMethod, testMethod.Method.Name);
                tasksByTestCaseUniqueId.Add(testCase.UniqueID, task);
            }
        }

        public RemoteTaskWrapper GetRemoteTask(ITestClassMessage testClass)
        {
            return GetClassTask(testClass.TestClass);
        }

        public RemoteTaskWrapper GetRemoteTask(string typeName)
        {
            return GetClassTask(typeName);
        }

        public RemoteTaskWrapper GetRemoteTask(ITestMethodMessage testMethod)
        {
            ITestMethod testMethod1 = testMethod.TestMethod;
            return GetMethodTask(testMethod1, testMethod1.Method.Name);
        }

        public RemoteTaskWrapper GetRemoteTask(ITestCaseMessage testCase)
        {
            lock (lockObject)
            {
                // All test cases have been discovered, so there should be no surprises here
                // (Dynamic theories will appear as unknown ITest instances, from a known ITestCase that maps to a method)
                RemoteTaskWrapper task;
                return tasksByTestCaseUniqueId.TryGetValue(testCase.TestCase.UniqueID, out task) ? task : null;
            }
        }

        public RemoteTaskWrapper GetRemoteTask(ITestMessage test)
        {
            lock (lockObject)
            {
                // It's safe to look up by test instance, as it doesn't change during execution
                RemoteTaskWrapper task;
                if (tasksByTestInstance.TryGetValue(test.Test, out task))
                    return task;

                ITestMethod testMethod = test.TestMethod;
                task = IsTheory(test.Test) ? GetNextTheoryTask(test.TestMethod, test.Test.DisplayName) : GetNextMethodTask(testMethod);
                tasksByTestInstance.Add(test.Test, task);

                return task;
            }
        }

        // TODO: Yuck. I don't like these here
        // TODO: Repetition of key creation
        public bool HasMethodTask(string typeName, string method)
        {
            var fullyQualifiedMethodName = string.Format("{0}.{1}", typeName, method);

            lock (lockObject)
                return tasksByFullyQualifiedMethodName.ContainsKey(fullyQualifiedMethodName);
        }

        public bool HasTheoryTask(string displayName, string typeName, string methodName)
        {
            var prefix = typeName + ".";
            var theoryName = displayName.StartsWith(prefix) ? displayName.Substring(prefix.Length) : displayName;
            var key = string.Format("{0}.{1}-{2}", typeName, methodName, theoryName);

            lock (lockObject)
                return tasksByTheoryId.ContainsKey(key);
        }

        private void AddMethodTask(string key, string typeName, RemoteTaskWrapper task)
        {
            lock (lockObject)
            {
                tasksByFullyQualifiedMethodName.Add(key, task);
                tasksByClassName[typeName].AddChild(task);
            }
        }

        private void AddTheoryTask(string key, string fullyQualifiedMethodName, RemoteTaskWrapper task)
        {
            lock (lockObject)
            {
                tasksByTheoryId.Add(key, task);
                tasksByFullyQualifiedMethodName[fullyQualifiedMethodName].AddChild(task);
            }
        }

        private static bool IsTheory(ITest test)
        {
            var displayName = test.DisplayName;
            var fullyQualifiedName = test.TestCase.FullyQualifiedName();
            return displayName != fullyQualifiedName;
        }

        private static bool IsTheory(ITestCase testCase)
        {
            var displayName = testCase.DisplayName;
            var fullyQualifiedName = testCase.FullyQualifiedName();
            return displayName != fullyQualifiedName;
        }

        private RemoteTaskWrapper GetClassTask(ITestClass testClass)
        {
            return GetClassTask(testClass.Class.Name);
        }

        private RemoteTaskWrapper GetClassTask(string typeName)
        {
            lock (lockObject)
            {
                RemoteTaskWrapper task;
                return tasksByClassName.TryGetValue(typeName, out task) ? task : null;
            }
        }

        private RemoteTaskWrapper GetNextMethodTask(ITestMethod testMethod)
        {
            lock (lockObject)
            {
                var task = GetMethodTask(testMethod, testMethod.Method.Name);
                for (var i = 2; handledDynamicTasks.Contains(task.RemoteTask); i++)
                {
                    var numberedMethodName = string.Format("{0} [{1}]", testMethod.Method.Name, i);
                    task = GetMethodTask(testMethod, numberedMethodName);
                }

                handledDynamicTasks.Add(task.RemoteTask);
                return task;
            }
        }

        private RemoteTaskWrapper GetMethodTask(ITestMethod testMethod, string methodName)
        {
            var key = string.Format("{0}.{1}", testMethod.TestClass.Class.Name, methodName);

            lock (lockObject)
            {
                RemoteTaskWrapper task;
                if (tasksByFullyQualifiedMethodName.TryGetValue(key, out task))
                    return task;

                task = CreateDynamicMethodTask(testMethod, methodName);
                AddMethodTask(key, testMethod.TestClass.Class.Name, task);

                return task;
            }
        }

        private RemoteTaskWrapper CreateDynamicMethodTask(ITestMethod testMethod, string methodName)
        {
            var classTask = GetClassTask(testMethod.TestClass);
            var methodTask = new XunitTestMethodTask((XunitTestClassTask) classTask.RemoteTask, methodName, true, true);
            var task = new RemoteTaskWrapper(methodTask, server);
            server.CreateDynamicElement(methodTask);
            return task;
        }

        private RemoteTaskWrapper GetNextTheoryTask(ITestMethod testMethod, string displayName)
        {
            lock (lockObject)
            {
                // TODO: Add an upper limit?
                // Would need an exception thrown and handling for null tasks
                var task = GetTheoryTask(testMethod, displayName);
                for (var i = 2; handledDynamicTasks.Contains(task.RemoteTask); i++)
                {
                    var numberedDisplayName = string.Format("{0} [{1}]", displayName, i);
                    task = GetTheoryTask(testMethod, numberedDisplayName);
                }

                handledDynamicTasks.Add(task.RemoteTask);
                return task;
            }
        }

        private RemoteTaskWrapper GetTheoryTask(ITestMethod testMethod, string displayName)
        {
            // Normalise the display name to ensure it doesn't have a type prefix
            // TODO: Why do we care about normalising the name?
            // Would make the code easier if we didn't have to. Could pass ITestCase here
            var prefix = testMethod.TestClass.Class.Name + ".";
            var theoryName = displayName.StartsWith(prefix) ? displayName.Substring(prefix.Length) : displayName;

            var fullyQualifiedMethodName = testMethod.FullyQualifiedName();
            var key = string.Format("{0}-{1}", fullyQualifiedMethodName, theoryName);

            lock (lockObject)
            {
                RemoteTaskWrapper task;
                if (tasksByTheoryId.TryGetValue(key, out task))
                    return task;

                task = CreateDynamicTheoryTask(testMethod, theoryName);
                AddTheoryTask(key, fullyQualifiedMethodName, task);

                return task;
            }
        }

        private RemoteTaskWrapper CreateDynamicTheoryTask(ITestMethod testMethod, string theoryName)
        {
            var methodTask = GetMethodTask(testMethod, testMethod.Method.Name);
            var theoryTask = new XunitTestTheoryTask((XunitTestMethodTask) methodTask.RemoteTask,
                DisplayNameUtil.Escape(theoryName));
            var task = new RemoteTaskWrapper(theoryTask, server);
            server.CreateDynamicElement(theoryTask);
            return task;
        }
    }
}