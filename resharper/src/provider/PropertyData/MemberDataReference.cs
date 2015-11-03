using System;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Resolve;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider.PropertyData
{
    public class MemberDataReference : TreeReferenceBase<ILiteralExpression>, ICompleteableReference
    {
        private readonly ITypeElement typeElement;
        private readonly Func<ILiteralExpression, TreeTextRange> getTreeTextRange;
        private readonly Func<IClrDeclaredElement, ITypeConversionRule> getTypeConversionRule;
        private readonly ISymbolFilter exactNameFilter;
        private readonly ISymbolFilter propertyFilter;

        public MemberDataReference(ITypeElement typeElement, ILiteralExpression literal,
                                   Func<ILiteralExpression, TreeTextRange> getTreeTextRange,
                                   Func<IClrDeclaredElement, ITypeConversionRule> getTypeConversionRule)
            : base(literal)
        {
            this.typeElement = typeElement;
            this.getTreeTextRange = getTreeTextRange;
            this.getTypeConversionRule = getTypeConversionRule;

            exactNameFilter = new ExactNameFilter((string) myOwner.ConstantValue.Value);
            propertyFilter = new PredicateFilter(FilterToApplicableMembers);
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
                return newOwner.FindReference<MemberDataReference>() ?? this;
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

        private bool FilterToApplicableMembers(ISymbolInfo symbolInfo)
        {
            var declaredElement = symbolInfo.GetDeclaredElement() as ITypeMember;
            if (declaredElement == null)
                return false;

            var predefinedType = declaredElement.Module.GetPredefinedType();

            // TODO: Allow all members, and have a problem analyser to show wrong signatures
            if (declaredElement.GetAccessRights() != AccessRights.PUBLIC || !declaredElement.IsStatic)
                return false;

            var property = declaredElement as IProperty;
            var field = declaredElement as IField;
            var method = declaredElement as IMethod;
            if (property == null && field == null && method == null)
                return false;

            if (method is IAccessor)
                return false;

            var conversionRule = getTypeConversionRule(declaredElement);

            var genericEnumerableTypeElement = predefinedType.GenericIEnumerable.GetTypeElement();
            if (genericEnumerableTypeElement == null)
                return false;
            var objectArrayType = TypeFactory.CreateArrayType(predefinedType.Object, 1);
            var x = EmptySubstitution.INSTANCE.Extend(genericEnumerableTypeElement.TypeParameters, new IType[] { objectArrayType }).Apply(predefinedType.GenericIEnumerable);

            var type = declaredElement.Type();
            if (type == null)
                return false;

            return type.IsImplicitlyConvertibleTo(x, conversionRule);
        }
    }
}