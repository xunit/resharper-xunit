using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.Feature.Services.CSharp.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Parsing;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider.Categories
{
    [Language(typeof(CSharpLanguage))]
    public class CSharpCategoriesCompletionProvider : XunitCategoriesCompletionProviderBase<CSharpCodeCompletionContext>
    {
        protected override IReference GetAttributeTypeReference(ITreeNode treeNode)
        {
            var attribute = GetAttribute(treeNode);
            return attribute != null ? attribute.TypeReference : null;
        }

        private static IAttribute GetAttribute(ITreeNode treeNode)
        {
            var argument = GetArgument(treeNode);
            return AttributeNavigator.GetByArgument(argument as ICSharpArgument);
        }

        private static IArgument GetArgument(ITreeNode treeNode)
        {
            return CSharpArgumentNavigator.GetByValue(treeNode as ICSharpExpression ?? treeNode.Parent as ICSharpExpression);
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
            var csArgument = argument as ICSharpArgument;
            if (csArgument != null && csArgument.Expression != null && csArgument.Expression.IsConstantValue())
                return csArgument.Expression.ConstantValue.Value as string;
            return null;
        }
    }
}