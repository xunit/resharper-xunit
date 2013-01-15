using System;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public static class MethodInfoExtensions
    {
        public static MultiValueDictionary<string, string> SafelyGetTraits(this IMethodInfo methodInfo)
        {
            try
            {
                // GetTraits relies on our attribute parsing to get at a property
                // value. Unless that value is set in the attribute constructor
                // via a named parameter (property) or a parameter with a name
                // similar to the required property, we'll return null, and that
                // can cause exceptions. If we get an exception, fail as nicely
                // as we can, with an empty collection of traits
                return MethodUtility.GetTraits(methodInfo);
            }
            catch (Exception)
            {
                return new MultiValueDictionary<string, string>();
            }
        }
    }
}