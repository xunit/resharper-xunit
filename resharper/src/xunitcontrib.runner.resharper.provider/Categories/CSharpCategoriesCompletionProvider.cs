using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CSharp.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.CodeCompletion;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.Lookup;
using JetBrains.ReSharper.Feature.Services.Lookup.Impl;
using JetBrains.ReSharper.Feature.Services.UnitTesting;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Parsing;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider.Categories
{
    [Language(typeof(CSharpLanguage))]
    public class CSharpCategoriesCompletionProvider : ItemsProviderOfSpecificContext<CSharpCodeCompletionContext>
    {
        protected override bool IsAvailable(CSharpCodeCompletionContext context)
        {
            var completionType = context.BasicContext.CodeCompletionType;
            if (!completionType.Equals(CodeCompletionType.BasicCompletion)
                && !completionType.Equals(CodeCompletionType.SmartCompletion)
                && !completionType.Equals(CodeCompletionType.AutomaticCompletion))
            {
                return false;
            }

            var treeNode = context.BasicContext.File.FindNodeAt(context.BasicContext.CaretDocumentRange);
            if (treeNode == null)
                return false;

            var argument = CSharpArgumentNavigator.GetByValue(treeNode as ICSharpExpression ?? treeNode.Parent as ICSharpExpression);
            var attribute = AttributeNavigator.GetByArgument(argument);
            if (attribute == null || attribute.TypeReference == null)
                return false;

            var resolveResult = attribute.TypeReference.Resolve();
            var attrType = resolveResult.DeclaredElement as ITypeElement;
            if (attrType == null)
                return false;

            // It would be nice to support derived types, but how do we know the parameters?
            return attrType.GetClrName().FullName == "Xunit.TraitAttribute";
        }

        protected override bool AddLookupItems(CSharpCodeCompletionContext context, GroupedItemsCollector collector)
        {
            var range = new TextRange(context.BasicContext.CaretDocumentRange.TextRange.StartOffset);
            var marker = range.CreateRangeMarker(context.BasicContext.Document);
            var categoriesProvider = context.BasicContext.CompletionManager.Solution.GetComponent<IUnitTestingCategoriesProvider>();
            foreach (var category in GetCategories(context, categoriesProvider.Categories))
            {
                var item = new UnitTestCategoryLookupItem(category, categoriesProvider.Image, marker);
                item.InitializeRanges(EvaluateRanges(context), context.BasicContext);
                collector.AddAtDefaultPlace(item);
            }
            return true;
        }

        private static IEnumerable<string> GetCategories(CSharpCodeCompletionContext context, IEnumerable<string> categories)
        {
            var treeNode = context.BasicContext.File.FindNodeAt(context.BasicContext.CaretDocumentRange);
            if (treeNode != null)
            {
                var argument = CSharpArgumentNavigator.GetByValue(treeNode as ICSharpExpression ?? treeNode.Parent as ICSharpExpression);
                var attribute = AttributeNavigator.GetByArgument(argument);
                if (argument != null && argument.MatchingParameter != null && attribute != null)
                {
                    var parameter = argument.MatchingParameter.Element;
                    if (parameter.ShortName == "name")
                        return GetTraitNames(categories);

                    if (parameter.ShortName == "value")
                    {
                        var traitName = from arg in attribute.Arguments
                                        where arg.MatchingParameter.Element.ShortName == "name"
                                        select (string)arg.Value.ConstantValue.Value;
                        return GetTraitValues(traitName.FirstOrDefault(), categories);
                    }
                }
            }
            return Enumerable.Empty<string>();
        }

        private static IEnumerable<string> GetTraitNames(IEnumerable<string> categories)
        {
            var traitNames = from category in categories
                             let index = category.IndexOf('[')
                             where index != -1
                             select category.Remove(index);

            return new[] {"Category"}.Concat(traitNames).Distinct(StringComparer.InvariantCultureIgnoreCase);
        }

        private static IEnumerable<string> GetTraitValues(string name, IEnumerable<string> categories)
        {
            if (string.Compare(name, "category", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return from category in categories
                       where !category.Contains('[')
                       select category;
            }

            return from category in categories
                   let split = category.Split('[')
                   where split.Length > 1
                   let traitName = split[0]
                   let traitValue = split[1].Substring(0, split[1].Length - 1)
                   where traitName == name
                   select traitValue;
        }

        private static TextLookupRanges EvaluateRanges(CSharpCodeCompletionContext context)
        {
            var basicContext = context.BasicContext;
            var selectionRange = basicContext.SelectedRange.TextRange;
            var referenceRange = basicContext.CaretDocumentRange.TextRange;

            var offset = basicContext.CaretTreeOffset;
            var expression = basicContext.File.FindTokenAt(offset) as ITokenNode;
            if (expression != null && expression.GetTokenType() == CSharpTokenType.STRING_LITERAL)
            {
                referenceRange = expression.GetDocumentRange().TextRange;
            }

            var replaceRange = new TextRange(referenceRange.StartOffset, Math.Max(referenceRange.EndOffset, selectionRange.EndOffset));
            var insertRange = replaceRange;

            return new TextLookupRanges(insertRange, false, replaceRange);
        }

        protected override LookupFocusBehaviour GetLookupFocusBehaviour(CSharpCodeCompletionContext context)
        {
            if (context.BasicContext.CodeCompletionType.Equals(CodeCompletionType.AutomaticCompletion))
                return LookupFocusBehaviour.Soft;
            return base.GetLookupFocusBehaviour(context);
        }
    }
}