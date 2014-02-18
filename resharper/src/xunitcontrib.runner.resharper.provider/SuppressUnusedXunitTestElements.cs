using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.ReSharper.Daemon.UsageChecking;
using JetBrains.ReSharper.Psi;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    // TODO: What version of ReSharper requires this? For any other reason?
    // TODO: Also, is this the only way? It's not nice from a performance perspective
    // Methods marked with [Fact] are marked in use because the external annotations for FactAttribute
    // give it [MeansImplicitUse]. It would appear that ReSharper automatically marks the test class
    // as in use (I don't know what version this started with - that was the reason for this class
    // in the first place)
    // Classes that are derived from test classes don't get marked as is use, so we'll suppress
    // those warnings here
    [ShellComponent]
    public class SuppressUnusedXunitTestElements : IUsageInspectionsSuppressor
    {
        // Bah humbug typos fixed between versions
        public bool SupressUsageInspectionsOnElement(IDeclaredElement element, out ImplicitUseKindFlags flags)
        {
            return SuppressUsageInspectionsOnElement(element, out flags);
        }

        public bool SuppressUsageInspectionsOnElement(IDeclaredElement element, out ImplicitUseKindFlags flags)
        {
            flags = 0;
            var suppress = element.IsAnyUnitTestElement();
            if (suppress)
                flags = ImplicitUseKindFlags.Default;
            return suppress;
        }
    }
}
