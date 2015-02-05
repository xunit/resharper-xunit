using System;
using System.Linq;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.VB;
using Xunit.Sdk;

using ICSharpConstructorDeclaration = JetBrains.ReSharper.Psi.CSharp.Tree.IConstructorDeclaration;
using IVBConstructorDeclaration = JetBrains.ReSharper.Psi.VB.Tree.IConstructorDeclaration;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    internal class PsiAttributeInfoAdapter : IAttributeInfo
    {
        readonly IAttributeInstance attribute;

        public PsiAttributeInfoAdapter(IAttributeInstance attribute)
        {
            this.attribute = attribute;
        }

        public T GetInstance<T>() where T : Attribute
        {
            return null;
        }

        public TValue GetPropertyValue<TValue>(string propertyName)
        {
            // Uh-oh. We're trying to get a property, but ReSharper only gives us
            // access to the constructor args that are set (what the property is
            // set to depends on the code in the constructor). If the constructor
            // is using named args, we're ok, because the name is the property name.
            // If we're not, and we're using positional parameters, we pretty much
            // have to guess. We can make a reasonable assumption that the parameter
            // name is going to match the property name, but probably in a different
            // case. This isn't ideal, but it's the only way we're going to get
            // Traits working
            var attributeValue = attribute.NamedParameter(propertyName);
            if (attributeValue.IsBadValue)
            {
                attributeValue = GetAttributeValueFromParameters(propertyName);
                if (attributeValue.IsBadValue)
                    attributeValue = GetAttributeValueFromBaseInitialiser(propertyName);
            }

            if (!attributeValue.IsBadValue && attributeValue.IsConstant)
                return (TValue)attributeValue.ConstantValue.Value;

            return default(TValue);
        }

        private AttributeValue GetAttributeValueFromParameters(string propertyName)
        {
            if (attribute.Constructor == null)
                return AttributeValue.BAD_VALUE;

            var parameters = attribute.Constructor.Parameters;
            for (var i = 0; i < parameters.Count; i++)
            {
                if (string.Compare(parameters[i].ShortName, propertyName, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    return attribute.PositionParameter(i);
                }
            }

            return AttributeValue.BAD_VALUE;
        }

        private AttributeValue GetAttributeValueFromBaseInitialiser(string propertyName)
        {
            if (attribute.Constructor == null)
                return AttributeValue.BAD_VALUE;

            var ctor = attribute.Constructor.GetDeclarations().FirstOrDefault();
            if (ctor == null)
                return AttributeValue.BAD_VALUE;

            if (ctor is ICSharpConstructorDeclaration)
                return GetAttributeValueFromCSharpInitiailiser(ctor as ICSharpConstructorDeclaration, propertyName);

            if (ctor is IVBConstructorDeclaration)
                return GetAttributeValueFromVBInitiailiser(ctor as IVBConstructorDeclaration, propertyName);

            return AttributeValue.BAD_VALUE;
        }

        private AttributeValue GetAttributeValueFromCSharpInitiailiser(ICSharpConstructorDeclaration declaration, string propertyName)
        {
            var initialiser = declaration.Initializer;
            if (initialiser == null || initialiser.Kind == ConstructorInitializerKind.UNKNOWN
                ||  initialiser.Reference == null)
            {
                return AttributeValue.BAD_VALUE;
            }

            var ctorReference = initialiser.Reference.Resolve();
            if (!ctorReference.IsValid() || ctorReference.ResolveErrorType != ResolveErrorType.OK)
                return AttributeValue.BAD_VALUE;

            var ctor = ctorReference.DeclaredElement as IConstructor;
            if (ctor == null)
                return AttributeValue.BAD_VALUE;

            foreach (var argument in initialiser.ArgumentsEnumerable)
            {
                if (ArgumentNameMatches(argument, propertyName) || ParameterNameMatches(argument, propertyName))
                {
                    if (argument.Value.IsConstantValue())
                        return new AttributeValue(argument.Value.ConstantValue);
                }
            }

            var nextCtor = ctor.GetDeclarations().OfType<ICSharpConstructorDeclaration>().FirstOrDefault();
            if (nextCtor != null)
                return GetAttributeValueFromCSharpInitiailiser(nextCtor, propertyName);

            return AttributeValue.BAD_VALUE;
        }

        private static bool ArgumentNameMatches(ICSharpArgumentInfo argument, string propertyName)
        {
            return string.Compare(argument.ArgumentName, propertyName, StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        private static bool ParameterNameMatches(ICSharpArgument argument, string propertyName)
        {
            return argument.MatchingParameter != null &&
                   string.Compare(argument.MatchingParameter.Element.ShortName, propertyName,
                       StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        private AttributeValue GetAttributeValueFromVBInitiailiser(IVBConstructorDeclaration declaration, string propertyName)
        {
            var initialiser = declaration.Initializer;
            if (initialiser == null || initialiser.Reference == null)
                return AttributeValue.BAD_VALUE;

            var ctorReference = initialiser.Reference.Resolve();
            if (!ctorReference.IsValid() || ctorReference.ResolveErrorType != ResolveErrorType.OK)
                return AttributeValue.BAD_VALUE;

            var ctor = ctorReference.DeclaredElement as IConstructor;
            if (ctor == null)
                return AttributeValue.BAD_VALUE;

            foreach (var argument in initialiser.ArgumentsEnumerable)
            {
                if (ArgumentNameMatches(argument, propertyName) || ParameterNameMatches(argument, propertyName))
                {
                    if (argument.Expression != null && argument.Expression.IsConstantValue())
                        return new AttributeValue(argument.Expression.ConstantValue);
                }
            }

            var nextCtor = ctor.GetDeclarations().OfType<IVBConstructorDeclaration>().FirstOrDefault();
            if (nextCtor != null)
                return GetAttributeValueFromVBInitiailiser(nextCtor, propertyName);

            return AttributeValue.BAD_VALUE;
        }

        private static bool ArgumentNameMatches(IVBArgumentInfo argument, string propertyName)
        {
            return string.Compare(argument.ArgumentName, propertyName, StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        private static bool ParameterNameMatches(IArgumentInfo argument, string propertyName)
        {
            return argument.MatchingParameter != null &&
                   string.Compare(argument.MatchingParameter.Element.ShortName, propertyName,
                       StringComparison.InvariantCultureIgnoreCase) == 0;
        }

    }
}