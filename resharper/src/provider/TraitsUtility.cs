using System.Collections.Generic;
using JetBrains.Util;
using Xunit;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    // xunit provides MethodUtility.GetTraits, but doesn't give us anything for
    // classes. Also, it requires the trait name to be non-null or it throws.
    // That's fine if you're running tests, but no good if you're half way
    // through iterating available traits, 'cos you end up with nothing. So we'll
    // do it ourselves, with null checks
    public static class TraitsUtility
    {
        public static OneToSetMap<string, string> GetTraits(this ITypeInfo typeInfo)
        {
            var attributes = typeInfo.GetCustomAttributes(typeof(TraitAttribute));
            return GetTraitsFromAttributes(attributes);
        }

        public static OneToSetMap<string, string> GetTraits(this IMethodInfo methodInfo)
        {
            var attributes = methodInfo.GetCustomAttributes(typeof(TraitAttribute));
            var traits = GetTraitsFromAttributes(attributes);
            attributes = methodInfo.Class.GetCustomAttributes(typeof(TraitAttribute));
            return GetTraitsFromAttributes(attributes, traits);
        }

        private static OneToSetMap<string, string> GetTraitsFromAttributes(IEnumerable<IAttributeInfo> attributes)
        {
            var traits = new OneToSetMap<string, string>();
            GetTraitsFromAttributes(attributes, traits);
            return traits;
        }

        private static OneToSetMap<string, string> GetTraitsFromAttributes(IEnumerable<IAttributeInfo> attributes,
            OneToSetMap<string, string> traits)
        {
            foreach (var attributeInfo in attributes)
            {
                var name = attributeInfo.GetPropertyValue<string>("Name");
                var value = attributeInfo.GetPropertyValue<string>("Value");
                if (name != null && value != null)
                    traits.Add(name, value);
            }

            return traits;
        }
    }
}