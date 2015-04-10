using JetBrains.ReSharper.Feature.Services.CodeCompletion;
using JetBrains.ReSharper.Feature.Services.Lookup;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public static class CompatibilityExtensions
    {
        public static bool IsAutomaticCompletion(this CodeCompletionParameters parameters)
        {
            return parameters.LastCodeCompletionType == CodeCompletionType.AutomaticCompletion;
        }

        public static void Add(this GroupedItemsCollector collector, ILookupItem item)
        {
            collector.AddAtDefaultPlace(item);
        }
    }
}