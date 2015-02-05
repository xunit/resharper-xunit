using System.Linq;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider.PropertyData
{
    public partial class CSharpPropertyDataReferenceFactory : IReferenceFactory
    {
        public IReference[] GetReferences(ITreeNode element, IReference[] oldReferences)
        {
            var literal = element as ILiteralExpression;
            if (literal != null && literal.ConstantValue.Value is string)
            {
                var attribute = AttributeNavigator.GetByConstructorArgumentExpression(literal as ICSharpExpression);
                if (attribute != null)
                {
                    var @class = attribute.Name.Reference.Resolve().DeclaredElement as IClass;
                    if (@class != null && Equals(@class.GetClrName(), XunitTestProvider.PropertyDataAttribute))
                    {
                        var typeElement = (from a in attribute.PropertyAssignments
                                           where a.PropertyNameIdentifier.Name == "PropertyType"
                                           select GetTypeof(a.Source as ITypeofExpression)).FirstOrDefault();

                        var member = MethodDeclarationNavigator.GetByAttribute(attribute);
                        if (member != null && member.DeclaredElement != null && typeElement == null)
                            typeElement = member.DeclaredElement.GetContainingType();

                        if (typeElement == null)
                            return EmptyArray<IReference>.Instance;

                        var reference = new PropertyDataReference(typeElement, literal);

                        return oldReferences != null && oldReferences.Length == 1 && Equals(oldReferences[0], reference)
                                   ? oldReferences 
                                   : new IReference[] {reference};
                    }
                }
            }

            return EmptyArray<IReference>.Instance;
        }

        private static ITypeElement GetTypeof(ITypeofExpression typeofExpression)
        {
            if (typeofExpression != null)
            {
                var scalarType = typeofExpression.ArgumentType.GetScalarType();
                if (scalarType != null)
                    return scalarType.GetTypeElement();
            }

            return null;
        }
    }
}