using System.Linq;
using JetBrains.Metadata.Reader.API;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.VB.Tree;
using JetBrains.ReSharper.Psi.VB.Types;
using JetBrains.ReSharper.Psi.VB.Util;
using JetBrains.Util;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider.PropertyData
{
    public class VBPropertyDataReferenceFactory : VBMemberDataReferenceFactoryBase
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
            return new PropertyDataReference(typeElement, literalExpression, GetTreeTextRange, p => GetTypeConversionRule(literalExpression));
        }
    }

    public class VBMemberDataReferenceFactory : VBMemberDataReferenceFactoryBase
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
            return new MemberDataReference(typeElement, literalExpression, GetTreeTextRange, p => GetTypeConversionRule(literalExpression));
        }
    }

    public abstract class VBMemberDataReferenceFactoryBase : IReferenceFactory
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
                    if (@class != null && Equals(@class.GetClrName(), DataAttributeName))
                    {
                        var typeElement = (from a in attribute.Arguments
                                           where a is INamedArgument && a.ArgumentName == TypeMemberName
                                           select GetTypeof(a.Expression as IGetTypeExpression)).FirstOrDefault();

                        var member = MethodDeclarationNavigator.GetByAttribute(attribute) as ITypeMemberDeclaration;
                        if (member != null && member.DeclaredElement != null && typeElement == null)
                            typeElement = member.DeclaredElement.GetContainingType();

                        if (typeElement == null)
                            return EmptyArray<IReference>.Instance;

                        var reference = CreateReference(typeElement, literal);

                        return oldReferences != null && oldReferences.Length == 1 && Equals(oldReferences[0], reference)
                                   ? oldReferences
                                   : new[] { reference };
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

        protected abstract IReference CreateReference(ITypeElement typeElement, ILiteralExpression literalExpression);

        protected TreeTextRange GetTreeTextRange(ILiteralExpression literalExpression)
        {
            var vbLiteral = literalExpression as IVBLiteralExpression;
            if (vbLiteral != null)
            {
                var range = vbLiteral.GetStringLiteralContentTreeRange();
                if (range.Length != 0)
                    return range;
            }

            return TreeTextRange.InvalidRange;
        }

        protected ITypeConversionRule GetTypeConversionRule(ILiteralExpression literal)
        {
            return (literal as IVBTreeNode).GetTypeConversionRule();
        }
    }
}