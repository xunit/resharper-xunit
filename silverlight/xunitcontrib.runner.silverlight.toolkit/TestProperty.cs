using Microsoft.Silverlight.Testing.UnitTesting.Metadata;

namespace XunitContrib.Runner.Silverlight.Toolkit
{
    internal class TestProperty : ITestProperty
    {
        public TestProperty(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; private set; }
        public string Value { get; private set; }
    }
}