using JetBrains.ReSharper.Daemon.UsageChecking;
using JetBrains.ReSharper.Psi;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    // This class allows us to suppress the "not in use" messages on elements such as methods or
    // classes that Solution Wide Analysis picks up, such as test methods and classes. It is
    // instantiated by the usage checking daemon.
    // ReSharper 5.0 uses external annotations for mstest and nunit to add the MeansImplicitUseAttribute
    // annotation to the TestFixture and Test attributes (and equivalents). This is for optimisation
    // purposes - the check for annotations happens sooner in the process, and is more lightweight than
    // this code approach.
    // ReSharper 4.5 had an instance of a class very much like this, that interrogated all available
    // unit test providers to see if the element was in use. This no longer exists (since nunit and
    // mstest can make do with external annotations) so we now need to handle this explicitly.
    // We also use external annotations for xunit, applying the MeansImplicitUseAttribute to the
    // Fact attribute, so all methods marked with Fact and derived attributes (such as Theory) are
    // now marked as in use.
    // However, this doesn't cater for test classes, since they do not have attributes, so we have
    // this class, which allows for a more detailed analysis, and which can also mark parent classes
    // and properties used by Theory attributes as in use.
    //
    // I did open a bug in Jira (RSRP-101582) that complained that marking a test method or test class
    // as being in use didn't mark parent classes as also in use, however, at that time, I didn't
    // realise that messages were suppressed rather than elements marked as in use.
    [UsageInspectionsSupressor]
    public class SuppressUnusedXunitTestElements : IUsageInspectionsSupressor
    {
        public bool SupressUsageInspectionsOnElement(IDeclaredElement element)
        {
            return XunitTestProvider.IsUnitTestElement(element);
        }
    }
}
