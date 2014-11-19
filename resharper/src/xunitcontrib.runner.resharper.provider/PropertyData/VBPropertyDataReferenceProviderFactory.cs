using System.Linq;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.VB.Tree;
using JetBrains.Util;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider.PropertyData
{
    public class VBPropertyDataReferenceProviderFactory : IReferenceFactory
    {
        public IReference[] GetReferences(ITreeNode element, IReference[] oldReferences)
        {
            var literal = element as ILiteralExpression;
            if (literal != null && literal.ConstantValue.Value is string)
            {
                var agument = literal.Parent as IVBArgument;
                var attribute = AttributeNavigator.GetByArgument(agument);
                if (attribute != null)
                {
                    var @class = attribute.AttributeType.Reference.Resolve().DeclaredElement as IClass;
                    if (@class != null && Equals(@class.GetClrName(), XunitTestProvider.PropertyDataAttribute))
                    {
                        var typeElement = (from a in attribute.Arguments
                                           where a is INamedArgument && a.ArgumentName == "PropertyType"
                                           select GetTypeof(a.Expression as IGetTypeExpression)).FirstOrDefault();

                        var member = GetAppliedToMethodDeclaration(attribute);
                        if (member != null && member.MethodElement != null && typeElement == null)
                            typeElement = member.MethodElement.GetContainingType();

                        if (typeElement == null)
                            return EmptyArray<IReference>.Instance;

                        var reference = new PropertyDataReference(typeElement, literal);

                        return oldReferences != null && oldReferences.Length == 1 && Equals(oldReferences[0], reference)
                                   ? oldReferences
                                   : new IReference[] { reference };
                    }
                }
            }

            return EmptyArray<IReference>.Instance;
        }

        // Same
        public bool HasReference(ITreeNode element, IReferenceNameContainer names)
        {
            var literal = element as ILiteralExpression;
            if (literal != null && literal.ConstantValue.Value is string)
                return names.Contains((string)literal.ConstantValue.Value);
            return false;
        }

        // Same, but different IMethodDeclaration
        private static IMethodDeclaration GetAppliedToMethodDeclaration(ITreeNode node)
        {
            while (node.Parent != null)
            {
                var methodDeclaration = node.Parent as IMethodDeclaration;
                if (methodDeclaration != null)
                    return methodDeclaration;
                node = node.Parent;
            }

            return null;
        }

        // Same, but different IGetTypeExpression/ITypeofExpression. No common base types
        private static ITypeElement GetTypeof(IGetTypeExpression getTypeExpression)
        {
            if (getTypeExpression != null)
            {
                var scalarType = getTypeExpression.ArgumentType.GetScalarType();
                if (scalarType != null)
                    return scalarType.GetTypeElement();
            }

            return null;
        }
    }
}