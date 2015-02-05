using System.Collections.Generic;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.VB.Tree;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider.PropertyData
{
    public partial class VBPropertyDataReferenceProviderFactory
    {
        public bool HasReference(ITreeNode element, ICollection<string> names)
        {
            var literal = element as ILiteralExpression;
            if (literal != null && literal.ConstantValue.Value is string)
                return names.Contains((string)literal.ConstantValue.Value);
            return false;
        }

        private static IMethod GetAppliedToMethod(ITreeNode node)
        {
            while (node.Parent != null)
            {
                var methodDeclaration = node.Parent as IMethodDeclaration;
                if (methodDeclaration != null)
                    return methodDeclaration.DeclaredElement;
                node = node.Parent;
            }

            return null;
        }
    }
}