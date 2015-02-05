using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider.PropertyData
{
    public partial class VBPropertyDataReferenceProviderFactory
    {
        public bool HasReference(ITreeNode element, IReferenceNameContainer names)
        {
            var literal = element as ILiteralExpression;
            if (literal != null && literal.ConstantValue.Value is string)
                return names.Contains((string)literal.ConstantValue.Value);
            return false;
        }
    }
}