using System;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class Parameter
    {
        public Parameter(string name, Type type)
        {
            Name = name;
            Type = type;
        }

        public Type Type { get; set; }
        public string Name { get; set; }

        public static Parameter Create<T>(string name)
        {
            return new Parameter(name, typeof(T));
        }
    }
}