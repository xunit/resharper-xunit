using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

// TODO: These are xunit1 tests
#if false
namespace tests.xunit.unsupported
{
    // The default ordering is reflection order, which is top-down order of the source file.
    // Note that reflection order is not guaranteed to remain constant across all
    // versions and implementations of the CLR.

    // TEST: The class should be marked as a test class
    // TEST: Running the class runs all of the tests
    // TEST: All tests pass
    [PrioritizedFixture]
    public class DefaultOrdering
    {
        public static bool Test1Called;
        public static bool Test2Called;
        public static bool Test3Called;

        // TEST: The method should be marked as a test class
        // TEST: Can run individual test
        [Fact]
        public void Test1()
        {
            Test1Called = true;

            Assert.False(Test2Called);
            Assert.False(Test3Called);
        }

        // TEST: The method should be marked as a test class
        // TEST: Can run individual test
        [Fact]
        public void Test2()
        {
            Test2Called = true;

            Assert.True(Test1Called);
            Assert.False(Test3Called);
        }

        // TEST: The method should be marked as a test class
        // TEST: Can run individual test
        [Fact]
        public void Test3()
        {
            Test3Called = true;

            Assert.True(Test1Called);
            Assert.True(Test2Called);
        }
    }

    // The default priority is 0. Within a given priority, the tests are run in
    // reflection order, just like the example above. Since reflection order should
    // not be relied upon, use test priority to ensure certain classes of tests run
    // before others as appropriate.

    // TEST: The class should be marked as a test class
    // TEST: Running the class runs all of the tests
    // TEST: All tests pass
    [PrioritizedFixture]
    public class CustomOrdering
    {
        public static bool Test1Called;
        public static bool Test2Called;
        public static bool Test3Called;

        // TEST: The method should be marked as a test class
        // TEST: Can run individual test
        [Fact, TestPriority(5)]
        public void Test3()
        {
            Test3Called = true;

            Assert.True(Test1Called);
            Assert.True(Test2Called);
        }

        // TEST: The method should be marked as a test class
        // TEST: Can run individual test
        [Fact]
        public void Test2()
        {
            Test2Called = true;

            Assert.True(Test1Called);
            Assert.False(Test3Called);
        }

        // TEST: The method should be marked as a test class
        // TEST: Can run individual test
        [Fact, TestPriority(-5)]
        public void Test1()
        {
            Test1Called = true;

            Assert.False(Test2Called);
            Assert.False(Test3Called);
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TestPriorityAttribute : Attribute
    {
        public TestPriorityAttribute(int priority)
        {
            Priority = priority;
        }

        public int Priority { get; private set; }
    }

    public class PrioritizedFixtureAttribute : RunWithAttribute
    {
        public PrioritizedFixtureAttribute() : base(typeof (PrioritizedFixtureClassCommand))
        {
        }
    }

    public class PrioritizedFixtureClassCommand : ITestClassCommand
    {
        // Delegate most of the work to the existing TestClassCommand class so that we
        // can preserve any existing behavior (like supporting IUseFixture<T>).
        private readonly TestClassCommand cmd = new TestClassCommand();

        public object ObjectUnderTest
        {
            get { return cmd.ObjectUnderTest; }
        }

        public ITypeInfo TypeUnderTest
        {
            get { return cmd.TypeUnderTest; }
            set { cmd.TypeUnderTest = value; }
        }

        public int ChooseNextTest(ICollection<IMethodInfo> testsLeftToRun)
        {
            // Always run the next test in the list, since the list is already ordered
            return 0;
        }

        public Exception ClassFinish()
        {
            return cmd.ClassFinish();
        }

        public Exception ClassStart()
        {
            return cmd.ClassStart();
        }

        public IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo testMethod)
        {
            return cmd.EnumerateTestCommands(testMethod);
        }

        public IEnumerable<IMethodInfo> EnumerateTestMethods()
        {
            var sortedMethods = new SortedDictionary<int, List<IMethodInfo>>();

            foreach (IMethodInfo method in cmd.EnumerateTestMethods())
            {
                int priority = 0;

                foreach (IAttributeInfo attr in method.GetCustomAttributes(typeof (TestPriorityAttribute)))
                    priority = attr.GetPropertyValue<int>("Priority");

                GetOrCreate(sortedMethods, priority).Add(method);
            }

            foreach (int priority in sortedMethods.Keys)
                foreach (IMethodInfo method in sortedMethods[priority])
                    yield return method;
        }

        public bool IsTestMethod(IMethodInfo testMethod)
        {
            return cmd.IsTestMethod(testMethod);
        }

        // Dictionary helper method

        private static TValue GetOrCreate<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key)
            where TValue : new()
        {
            TValue result;

            if (!dictionary.TryGetValue(key, out result))
            {
                result = new TValue();
                dictionary[key] = result;
            }

            return result;
        }
    }
}
#endif