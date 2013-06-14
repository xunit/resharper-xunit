using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.Util;

#region Compatibility hacks
// ReSharper 8.0
// This namespace only exists pre-8.0, but is used in this file for IUnitTestingCategoriesProvider
// Delcaring an empty namespace gives us source code compatibility
namespace JetBrains.ReSharper.Feature.Services.UnitTesting
{
}

// ReSharper pre-8.0
// Similarly, this namespace doesn't exist pre 8.0
namespace JetBrains.ReSharper.Features.Shared.UnitTesting
{
}
#endregion

namespace XunitContrib.Runner.ReSharper.UnitTestProvider.Categories
{
    public abstract partial class XunitCategoriesCompletionProviderBase<T>
        where T : class, ISpecificCodeCompletionContext
    {
        private static TextLookupRanges CreateRanges(TextRange insertRange, TextRange replaceRange)
        {
            return new TextLookupRanges(insertRange, false, replaceRange);
        }
    }
}