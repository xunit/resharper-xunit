using System.Collections.Generic;
using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public partial class XunitBaseElement
    {
        public UnitTestElementId Id { get { return UnitTestElementId; } }

        protected static UnitTestElementNamespace GetNamespace(IEnumerable<string> namespaces)
        {
            return UnitTestElementNamespaceFactory.Create(namespaces);
        }
    }
}