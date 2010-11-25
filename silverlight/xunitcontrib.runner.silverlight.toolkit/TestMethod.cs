using System;
using System.Collections.Generic;
using System.Linq;
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
        private IList<string> categories;
        private IList<string> owners;
        private IList<string> descriptions;
        private IList<ITestProperty> testProperties;

        public TestMethod(ITestClassCommand testClassCommand, ITestCommand testCommand, IMethodInfo methodInfo)
        {
            this.testClassCommand = testClassCommand;
            this.testCommand = testCommand;
            this.methodInfo = methodInfo;

            HandleTraits();
        }

        private void HandleTraits()
        {
            var traits = MethodUtility.GetTraits(methodInfo);

            categories = ExtractTraitValues(traits, "category");
            owners = ExtractTraitValues(traits, "owner");
            descriptions = ExtractTraitValues(traits, "description");

            testProperties = ConvertToTestPropeties(traits);
        }

        private static IList<ITestProperty> ConvertToTestPropeties(MultiValueDictionary<string, string> traits)
        {
            var x = from key in traits.Keys
                    select new TestProperty(key, string.Join("; ", traits[key].ToArray())) as ITestProperty;
            return x.ToList();
        }

        private static IList<string> ExtractTraitValues(MultiValueDictionary<string, string> traits, string traitName)
        {
            var values = from key in traits.Keys
                         where string.Compare(key, traitName, StringComparison.OrdinalIgnoreCase) == 0
                         from value in traits[key]
                         select value;

            traits.Remove(traitName);

            return values.ToList();
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
            get { return Ignore ? MethodUtility.GetSkipReason(methodInfo) : descriptions.FirstOrDefault(); }
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

        // Not used?
        public string Category
        {
            get { return categories.FirstOrDefault(); }
        }

        // This only seems to be output to the log writers, not displayed on the screen
        public string Owner
        {
            get { return owners.FirstOrDefault(); }
        }

        public IExpectedException ExpectedException
        {
            get { return null; }
        }

        public int? Timeout
        {
            get { return testCommand.Timeout > 0 ? testCommand.Timeout : (int?) null; }
        }

        public ICollection<ITestProperty> Properties
        {
            get { return testProperties; }
        }

        // I don't think this is used anywhere
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