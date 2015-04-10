using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CodeCompletion;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems.Impl;
using JetBrains.ReSharper.Feature.Services.Lookup;
using JetBrains.ReSharper.Feature.Services.Lookup.Impl;
using JetBrains.ReSharper.Feature.Services.UnitTesting;
using JetBrains.ReSharper.Features.Shared.UnitTesting;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

// ReSharper 8.2 doesn't define these namespaces used by 9.0
namespace JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems
{
    namespace Impl
    {
    }
}
namespace JetBrains.ReSharper.Feature.Services.UnitTesting
{
}

// ReSharper 9.0 doesn't define these namespaces used by 8.2
namespace JetBrains.ReSharper.Feature.Services.Lookup.Impl
{
}
namespace JetBrains.ReSharper.Features.Shared.UnitTesting
{
}

namespace XunitContrib.Runner.ReSharper.UnitTestProvider.Categories
{
    public abstract class XunitCategoriesCompletionProviderBase<T> : ItemsProviderOfSpecificContext<T>
        where T : class, ISpecificCodeCompletionContext
    {
        protected override bool IsAvailable(T context)
        {
            var completionType = context.BasicContext.CodeCompletionType;
            if (!completionType.Equals(CodeCompletionType.BasicCompletion)
                && !completionType.Equals(CodeCompletionType.SmartCompletion)
                && !context.BasicContext.Parameters.IsAutomaticCompletion())
            {
                return false;
            }

            var attributeType = GetAttributeType(context);
            if (attributeType == null)
                return false;

            return string.Equals(attributeType.GetClrName().FullName, "Xunit.TraitAttribute",
                                 StringComparison.InvariantCultureIgnoreCase);
        }

        private ITypeElement GetAttributeType(T context)
        {
            var treeNode = context.BasicContext.File.FindNodeAt(context.BasicContext.CaretDocumentRange);
            if (treeNode == null)
                return null;

            var typeReference = GetAttributeTypeReference(treeNode);
            if (typeReference == null)
                return null;

            var resolveResult = typeReference.Resolve();
            return resolveResult.DeclaredElement as ITypeElement;
        }

        protected override bool AddLookupItems(T context, GroupedItemsCollector collector)
        {
            var range = new TextRange(context.BasicContext.CaretDocumentRange.TextRange.StartOffset);
            var marker = range.CreateRangeMarker(context.BasicContext.Document);
            var categoriesProvider = context.BasicContext.CompletionManager.Solution.GetComponent<IUnitTestingCategoriesProvider>();
            foreach (var category in GetCategories(context, categoriesProvider.Categories))
            {
                var item = new UnitTestCategoryLookupItem(category, categoriesProvider.Image, marker);
                item.InitializeRanges(EvaluateRanges(context), context.BasicContext);
                collector.Add(item);
            }
            return true;
        }

        protected override LookupFocusBehaviour GetLookupFocusBehaviour(T context)
        {
            if (context.BasicContext.Parameters.IsAutomaticCompletion())
                return LookupFocusBehaviour.Soft;
            return base.GetLookupFocusBehaviour(context);
        }

        private IEnumerable<string> GetCategories(T context, IEnumerable<string> categories)
        {
            var treeNode = context.BasicContext.File.FindNodeAt(context.BasicContext.CaretDocumentRange);
            if (treeNode == null)
                return Enumerable.Empty<string>();

            var parameterName = GetParameterName(treeNode);

            if (parameterName == "name")
                return GetTraitNames(categories);

            if (parameterName == "value")
            {
                var traitName = (from arg in GetAttributeArguments(treeNode)
                                 where arg.MatchingParameter != null && arg.MatchingParameter.Element.ShortName == "name"
                                 select GetConstantValue(arg)).FirstOrDefault();
                return GetTraitValues(traitName, categories);
            }

            return Enumerable.Empty<string>();
        }

        protected abstract string GetParameterName(ITreeNode treeNode);
        protected abstract IEnumerable<IArgument> GetAttributeArguments(ITreeNode treeNode);
        protected abstract string GetConstantValue(IArgument argument);

        private static IEnumerable<string> GetTraitNames(IEnumerable<string> categories)
        {
            var traitNames = from category in categories
                             let index = category.IndexOf('[')
                             where index != -1
                             select category.Remove(index);

            return new[] { "Category" }.Concat(traitNames).Distinct(StringComparer.InvariantCultureIgnoreCase);
        }

        private static IEnumerable<string> GetTraitValues(string name, IEnumerable<string> categories)
        {
            if (String.Compare(name, "category", StringComparison.InvariantCultureIgnoreCase) == 0)
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

        private static TextLookupRanges EvaluateRanges(T context)
        {
            var basicContext = context.BasicContext;
            var selectionRange = basicContext.SelectedRange.TextRange;
            var referenceRange = basicContext.CaretDocumentRange.TextRange;

            var offset = basicContext.CaretTreeOffset;
            var expression = basicContext.File.FindTokenAt(offset) as ITokenNode;
            if (expression != null && expression.GetTokenType().IsStringLiteral)
                referenceRange = expression.GetDocumentRange().TextRange;

            var replaceRange = new TextRange(
                referenceRange.StartOffset, Math.Max(referenceRange.EndOffset, selectionRange.EndOffset));
            var insertRange = replaceRange;

            return CreateRanges(insertRange, replaceRange);
        }

        protected abstract IReference GetAttributeTypeReference(ITreeNode treeNode);

        private static TextLookupRanges CreateRanges(TextRange insertRange, TextRange replaceRange)
        {
            return new TextLookupRanges(insertRange, replaceRange);
        }
    }
}