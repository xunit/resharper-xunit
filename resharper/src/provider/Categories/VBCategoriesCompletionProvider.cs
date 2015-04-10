using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.Feature.Services.VB.CodeCompletion;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.VB;
using JetBrains.ReSharper.Psi.VB.Tree;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider.Categories
{
    [Language(typeof(VBLanguage))]
    public class VBCategoriesCompletionProvider : XunitCategoriesCompletionProviderBase<VBCodeCompletionContextBase>
    {
        protected override IReference GetAttributeTypeReference(ITreeNode treeNode)
        {
            var attribute = GetAttribute(treeNode);
            return attribute != null ? attribute.TypeReference : null;
        }

        private static IAttribute GetAttribute(ITreeNode treeNode)
        {
            var argument = GetArgument(treeNode);
            return AttributeNavigator.GetByArgument(argument);
        }

        private static IVBArgument GetArgument(ITreeNode treeNode)
        {
            return VBArgumentNavigator.GetByExpression(treeNode as IVBExpression ?? treeNode.Parent as IVBExpression);
        }

        protected override string GetParameterName(ITreeNode treeNode)
        {
            var argument = GetArgument(treeNode);
            if (argument != null && argument.MatchingParameter != null)
                return argument.MatchingParameter.Element.ShortName;
            return string.Empty;
        }

        protected override IEnumerable<IArgument> GetAttributeArguments(ITreeNode treeNode)
        {
            var attribute = GetAttribute(treeNode);
            return attribute != null ? attribute.Arguments.Cast<IArgument>() : Enumerable.Empty<IArgument>();
        }

        protected override string GetConstantValue(IArgument argument)
        {
            var expressionArgument = argument as IExpressionArgument;
            if (expressionArgument != null && expressionArgument.Expression.IsConstantValue())
                return expressionArgument.Expression.ConstantValue.Value as string;
            return null;
        }
    }
}