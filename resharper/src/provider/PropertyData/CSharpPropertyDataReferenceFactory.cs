using System.Linq;
using JetBrains.Metadata.Reader.API;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Impl;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.CSharp.Util;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider.PropertyData
{
    public class CSharpPropertyDataReferenceFactory : CSharpMemberDataReferenceFactoryBase
    {
        protected override IClrTypeName DataAttributeName
        {
            get { return XunitTestProvider.PropertyDataAttribute; }
        }

        protected override string TypeMemberName
        {
            get { return "PropertyType"; }
        }

        protected override IReference CreateReference(ITypeElement typeElement, ILiteralExpression literalExpression)
        {
            return new PropertyDataReference(typeElement, literalExpression, GetTreeTextRange, GetTypeConversionRule);
        }
    }

    public class CSharpMemberDataReferenceFactory : CSharpMemberDataReferenceFactoryBase
    {
        protected override IClrTypeName DataAttributeName
        {
            get { return XunitTestProvider.MemberDataAttribute; }
        }

        protected override string TypeMemberName
        {
            get { return "MemberType"; }
        }

        protected override IReference CreateReference(ITypeElement typeElement, ILiteralExpression literalExpression)
        {
            return new MemberDataReference(typeElement, literalExpression, GetTreeTextRange, GetTypeConversionRule);
        }
    }


    public abstract class CSharpMemberDataReferenceFactoryBase : IReferenceFactory
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
                    if (@class != null && Equals(@class.GetClrName(), DataAttributeName))
                    {
                        var typeElement = (from a in attribute.PropertyAssignments
                                           where a.PropertyNameIdentifier.Name == TypeMemberName
                                           select GetTypeof(a.Source as ITypeofExpression)).FirstOrDefault();

                        var member = MethodDeclarationNavigator.GetByAttribute(attribute);
                        if (member != null && member.DeclaredElement != null && typeElement == null)
                            typeElement = member.DeclaredElement.GetContainingType();

                        if (typeElement == null)
                            return EmptyArray<IReference>.Instance;

                        var reference = CreateReference(typeElement, literal);

                        return oldReferences != null && oldReferences.Length == 1 && Equals(oldReferences[0], reference)
                                   ? oldReferences 
                                   : new[] {reference};
                    }
                }
            }

            return EmptyArray<IReference>.Instance;
        }

        public bool HasReference(ITreeNode element, IReferenceNameContainer names)
        {
            var literal = element as ILiteralExpression;
            if (literal != null && literal.ConstantValue.Value is string)
                return names.Contains((string)literal.ConstantValue.Value);
            return false;
        }

        protected abstract IClrTypeName DataAttributeName { get; }
        protected abstract string TypeMemberName { get; }

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

        protected abstract IReference CreateReference(ITypeElement typeElement, ILiteralExpression literalExpression);

        protected TreeTextRange GetTreeTextRange(ILiteralExpression literalExpression)
        {
            var csharpLiteral = literalExpression as ICSharpLiteralExpression;
            if (csharpLiteral != null)
            {
                var range = csharpLiteral.GetStringLiteralContentTreeRange();
                if (range.Length != 0)
                    return range;
            }

            return TreeTextRange.InvalidRange;
        }

        protected ITypeConversionRule GetTypeConversionRule(IClrDeclaredElement declaredElement)
        {
            return new CSharpTypeConversionRule(declaredElement.Module);
        }

    }
}