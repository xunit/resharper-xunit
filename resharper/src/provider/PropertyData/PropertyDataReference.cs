using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Impl;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Resolve;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.ReSharper.Psi.CSharp.Util;
using JetBrains.ReSharper.Psi.VB.Impl.DeclaredElements;
using JetBrains.ReSharper.Psi.VB.Tree;
using JetBrains.ReSharper.Psi.VB.Types;
using JetBrains.ReSharper.Psi.VB.Util;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider.PropertyData
{
    public class PropertyDataReference : TreeReferenceBase<ILiteralExpression>, ICompleteableReference
    {
        private readonly ITypeElement typeElement;
        private readonly ISymbolFilter exactNameFilter;
        private readonly ISymbolFilter propertyFilter;

        public PropertyDataReference(ITypeElement typeElement, ILiteralExpression literal)
            : base(literal)
        {
            this.typeElement = typeElement;

            exactNameFilter = new ExactNameFilter((string) myOwner.ConstantValue.Value);
            propertyFilter = new PredicateFilter(info => FilterToApplicableProperties(info, literal));
        }

        public override ResolveResultWithInfo ResolveWithoutCache()
        {
            var resolveResult = GetReferenceSymbolTable(true).GetResolveResult(GetName());

            return Equals(resolveResult.Info.ResolveErrorType, ResolveErrorType.MULTIPLE_CANDIDATES)
                       ? new ResolveResultWithInfo(resolveResult.Result, ResolveErrorType.OK)
                       : resolveResult; 
        }

        public override string GetName()
        {
            return (string) myOwner.ConstantValue.Value;
        }

        public override ISymbolTable GetReferenceSymbolTable(bool useReferenceName)
        {
            var symbolTable = ResolveUtil.GetSymbolTableByTypeElement(typeElement, SymbolTableMode.FULL, typeElement.Module);

            // The NUnit code gets inheritors here, and adds all of those symbols, but I don't
            // think that makes sense for xunit - the property needs to be a static property,
            // so must be defined on this type or base types, which are already included

            symbolTable = symbolTable.Distinct().Filter(propertyFilter);

            return useReferenceName ? symbolTable.Filter(GetName(), exactNameFilter) : symbolTable;
        }

        public override TreeTextRange GetTreeTextRange()
        {
            var csharpLiteral = myOwner as ICSharpLiteralExpression;
            if (csharpLiteral != null)
            {
                var range = csharpLiteral.GetStringLiteralContentTreeRange();
                if (range.Length != 0)
                    return range;
            }

            var vbLiteral = myOwner as IVBLiteralExpression;
            if (vbLiteral != null)
            {
                var range = vbLiteral.GetStringLiteralContentTreeRange();
                if (range.Length != 0)
                    return range;
            }

            return myOwner.GetTreeTextRange();
        }

        public override IReference BindTo(IDeclaredElement element)
        {
            var literalAlterer = StringLiteralAltererUtil.CreateStringLiteralByExpression(myOwner);
            literalAlterer.Replace((string) myOwner.ConstantValue.Value, element.ShortName, myOwner.GetPsiModule());
            var newOwner = literalAlterer.Expression;
            if (!myOwner.Equals(newOwner))
                return newOwner.FindReference<PropertyDataReference>() ?? this;
            return this;
        }

        public override IReference BindTo(IDeclaredElement element, ISubstitution substitution)
        {
            return BindTo(element);
        }

        public override IAccessContext GetAccessContext()
        {
            return new ElementAccessContext(myOwner);
        }

        public ISymbolTable GetCompletionSymbolTable()
        {
            return GetReferenceSymbolTable(false).Filter(propertyFilter);
        }

        private static bool FilterToApplicableProperties(ISymbolInfo symbolInfo, ILiteralExpression literal)
        {
            var declaredElement = symbolInfo.GetDeclaredElement() as ITypeMember;
            if (declaredElement == null)
                return false;

            var predefinedType = declaredElement.Module.GetPredefinedType(declaredElement.ResolveContext);

            var property = declaredElement as IProperty;
            if (property == null)
                return false;

            if (property.GetAccessRights() != AccessRights.PUBLIC || !property.IsStatic)
                return false;

            ITypeConversionRule conversionRule;
            if (property is IVBProperty)
                conversionRule = (literal as IVBTreeNode).GetTypeConversionRule();
            else
                conversionRule = new CSharpTypeConversionRule(property.Module);

            var genericEnumerableTypeElement = predefinedType.GenericIEnumerable.GetTypeElement();
            if (genericEnumerableTypeElement == null)
                return false;
            var objectArrayType = TypeFactory.CreateArrayType(predefinedType.Object, 1);
            var x = EmptySubstitution.INSTANCE.Extend(genericEnumerableTypeElement.TypeParameters, new[] { objectArrayType }).Apply(predefinedType.GenericIEnumerable);

            return property.Type.IsImplicitlyConvertibleTo(x, conversionRule);
        }
    }
}