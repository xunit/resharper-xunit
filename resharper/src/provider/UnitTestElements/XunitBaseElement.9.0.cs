using System.Collections.Generic;
using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public partial class XunitBaseElement
    {
        public UnitTestElementId Id { get { return UnitTestElementId; } }

        protected static UnitTestNamespace GetNamespace(IEnumerable<string> namespaces)
        {
            return new UnitTestNamespace(namespaces);
        }
    }
}