using System;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Resolve;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider.PropertyData
{
    public class PropertyDataReference : TreeReferenceBase<ILiteralExpression>, ICompleteableReference
    {
        private readonly ITypeElement typeElement;
        private readonly Func<ILiteralExpression, TreeTextRange> getTreeTextRange;
        private readonly Func<IProperty, ITypeConversionRule> getTypeConversionRule;
        private readonly ISymbolFilter exactNameFilter;
        private readonly ISymbolFilter propertyFilter;

        public PropertyDataReference(ITypeElement typeElement, ILiteralExpression literal,
                                     Func<ILiteralExpression, TreeTextRange> getTreeTextRange,
                                     Func<IProperty, ITypeConversionRule> getTypeConversionRule)
            : base(literal)
        {
            this.typeElement = typeElement;
            this.getTreeTextRange = getTreeTextRange;
            this.getTypeConversionRule = getTypeConversionRule;

            exactNameFilter = new ExactNameFilter((string) myOwner.ConstantValue.Value);
            propertyFilter = new PredicateFilter(FilterToApplicableProperties);
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
            var range = getTreeTextRange(myOwner);
            return range.IsValid() ? range : myOwner.GetTreeTextRange();
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

        private bool FilterToApplicableProperties(ISymbolInfo symbolInfo)
        {
            var declaredElement = symbolInfo.GetDeclaredElement() as ITypeMember;
            if (declaredElement == null)
                return false;

            var predefinedType = declaredElement.Module.GetPredefinedType();

            var property = declaredElement as IProperty;
            if (property == null)
                return false;

            if (property.GetAccessRights() != AccessRights.PUBLIC || !property.IsStatic)
                return false;

            var conversionRule = getTypeConversionRule(property);

            var genericEnumerableTypeElement = predefinedType.GenericIEnumerable.GetTypeElement();
            if (genericEnumerableTypeElement == null)
                return false;
            var objectArrayType = TypeFactory.CreateArrayType(predefinedType.Object, 1);
            var x = EmptySubstitution.INSTANCE.Extend(genericEnumerableTypeElement.TypeParameters, new IType[] { objectArrayType }).Apply(predefinedType.GenericIEnumerable);

            return property.Type.IsImplicitlyConvertibleTo(x, conversionRule);
        }
    }
}