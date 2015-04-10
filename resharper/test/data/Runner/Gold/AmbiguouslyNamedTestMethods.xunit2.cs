using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Foo
{
    // xunit2 supports this just fine, but we need to make sure
    // the methods are called in a consistent order
    [TestCaseOrderer("PriorityOrderer", "AmbiguouslyNamedTestMethods.xunit2")]
    public class AmbiguouslyNamedTestMethods
    {
        [Theory]
        [InlineData(12)]
        [TestPriority(1)]
        public void TestMethod1(int value)
        {
        }

        [Theory]
        [InlineData("foo")]
        [TestPriority(2)]
        public void TestMethod1(string value)
        {
        }
    }
}

// From https://github.com/xunit/samples.xunit/tree/d9dcdba0fe5b8513bf8d75773a12758fe5f36adf/TestOrderExamples/TestCaseOrdering
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class TestPriorityAttribute : Attribute
{
    public TestPriorityAttribute(int priority)
    {
        Priority = priority;
    }

    public int Priority { get; private set; }
}

public class PriorityOrderer : ITestCaseOrderer
{
    public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
    {
        var sortedMethods = new SortedDictionary<int, List<TTestCase>>();

        foreach (TTestCase testCase in testCases)
        {
            int priority = 0;

            foreach (IAttributeInfo attr in testCase.TestMethod.Method.GetCustomAttributes((typeof (TestPriorityAttribute).AssemblyQualifiedName)))
                priority = attr.GetNamedArgument<int>("Priority");

            GetOrCreate(sortedMethods, priority).Add(testCase);
        }

        foreach (var list in sortedMethods.Keys.OrderBy(k => k).Select(priority => sortedMethods[priority]))
        {
            list.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.TestMethod.Method.Name, y.TestMethod.Method.Name));
            foreach (TTestCase testCase in list)
                yield return testCase;
        }
    }

    static TValue GetOrCreate<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
    {
        TValue result;

        if (dictionary.TryGetValue(key, out result)) return result;
        
        result = new TValue();
        dictionary[key] = result;

        return result;
    }
}
