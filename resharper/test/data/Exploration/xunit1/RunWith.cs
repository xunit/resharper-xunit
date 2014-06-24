using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

namespace Foo
{
    // The class will be marked as a test class, but without any methods
    // although they will be found at runtime
    [MyRunWith]
    public class TestClass
    {
        public void Test1()
        {
        }

        public void Test2()
        {
        }
    }

    // TODO: Both the test class and test methods are discovered
    [MyRunWith]
    public class TestClass2
    {
        [Fact]
        public void Test1()
        {
        }

        [Fact]
        public void Test2()
        {
        }
    }

    public class MyRunWithAttribute : RunWithAttribute
    {
        public MyRunWithAttribute()
            : base(typeof (MyRunWithClassCommand))
        {
        }
    }

    public class MyRunWithClassCommand : ITestClassCommand
    {
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
            yield return new FactCommand(testMethod);
        }

        public IEnumerable<IMethodInfo> EnumerateTestMethods()
        {
            foreach (var method in TypeUnderTest.GetMethods())
            {
                if (method.Name.StartsWith("Test"))
                    yield return method;
            }
        }

        public bool IsTestMethod(IMethodInfo testMethod)
        {
            return testMethod.Name.StartsWith("Test");
        }
    }
}
