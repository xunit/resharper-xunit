using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using Xunit.Sdk;

namespace XunitContrib.Runner.Silverlight.Toolkit
{
    public class TestMethod : ITestMethod
    {
        private readonly ITestClassCommand testClassCommand;
        private readonly ITestCommand testCommand;
        private readonly IMethodInfo methodInfo;

        public TestMethod(ITestClassCommand testClassCommand, ITestCommand testCommand, IMethodInfo methodInfo)
        {
            this.testClassCommand = testClassCommand;
            this.testCommand = testCommand;
            this.methodInfo = methodInfo;
        }

        public MethodInfo Method
        {
            get { return methodInfo.MethodInfo; }
        }

        public void DecorateInstance(object instance)
        {
        }

        // TODO: Route all output through here
        public event EventHandler<StringEventArgs> WriteLine;

        public bool Ignore
        {
            get { return MethodUtility.IsSkip(methodInfo); }
        }

        public string Description
        {
            get { return Ignore ? MethodUtility.GetSkipReason(methodInfo) : null; }
        }

        public string Name
        {
            get
            {
                // The Silverlight UTF is expecting a short name here. But xunit might be giving
                // us a specially formatted display name. Check for the full method name, and
                // replace with the short name if we find a match
                var displayName = testCommand.DisplayName;
                if (displayName.StartsWith(methodInfo.TypeName + "."))
                    displayName = displayName.Remove(0, methodInfo.TypeName.Length + 1);
                return displayName;
            }
        }

        public string Category
        {
            get { return null; }
        }

        public string Owner
        {
            get { return null; }
        }

        public IExpectedException ExpectedException
        {
            get { return null; }
        }

        public int? Timeout
        {
            get { return null; }
        }

        public ICollection<ITestProperty> Properties
        {
            get { return null; }
        }

        public ICollection<IWorkItemMetadata> WorkItems
        {
            get { return null; }
        }

        public IPriority Priority
        {
            get { return null; }
        }

        public IEnumerable<Attribute> GetDynamicAttributes()
        {
            return null;
        }

        public void Invoke(object instance)
        {
            var methodResult = testCommand.Execute(testClassCommand.ObjectUnderTest);
            var failedResult = methodResult as ExceptionResult;
            if (failedResult != null)
            {
                throw new TargetInvocationException(failedResult.Exception);
            }
        }
    }
}