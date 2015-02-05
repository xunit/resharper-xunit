using System.Collections.Generic;
using JetBrains.ReSharper.Psi.Tree;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider.PropertyData
{
    public partial class CSharpPropertyDataReferenceFactory
    {
        public bool HasReference(ITreeNode element, ICollection<string> names)
        {
            var literal = element as ILiteralExpression;
            if (literal != null && literal.ConstantValue.Value is string)
                return names.Contains((string)literal.ConstantValue.Value);
            return false;
        }
    }
}