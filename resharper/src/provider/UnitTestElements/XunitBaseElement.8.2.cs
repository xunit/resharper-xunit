using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public partial class XunitBaseElement
    {
        public string GetPresentation(IUnitTestElement element)
        {
            return GetPresentation(element, false);
        }

        public string Id { get { return UnitTestElementId.ToString(); } }
        public IUnitTestProvider Provider { get { return UnitTestElementId.Provider; } }

        protected static UnitTestNamespace GetNamespace(IEnumerable<string> namespaces)
        {
            return new UnitTestNamespace(string.Join(".", namespaces.ToArray()));
        }
    }
}