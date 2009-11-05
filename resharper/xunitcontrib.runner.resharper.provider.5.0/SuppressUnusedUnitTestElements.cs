using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon.UsageChecking;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    // Solution Wide Analysis will flag any public methods or classes that aren't used as warnings.
    // Ideally, ReSharper would check for test methods and if a test method is in use, then its
    // containing class would be in use, and that class's parent class would also be in use and so on.
    // Unfortunately, it's not that simple.
    // Unless an element is actually used, any means of marking it as being "in use" is an exception. In
    // other words, applying the UsedImplicitlyAttribute or applying the MeansImplicitUseAttribute to an
    // attribute *suppresses* the warning message. Which means we need to suppress the message for any
    // containing or parent elements. If applying these attributes caused ReSharper to treat the element
    // as in use, the containing and parent classes would also be flagged as in use automatically.
    // Thus is an outstanding bug report on Jira for this - RSRP-101582
    //
    // The nunit and mstest providers work by applying the MeansImplicitUseAttribute to the TestFixture
    // and Test attribuetes (or equivalent) via the external annotations files. This won't work for xunit
    // for several reasons:
    // 1) There is no class level attribute, so test classes would always be marked as unused
    // 2) It won't work for custom Fact or Theory attributes
    // 3) It won't be able to mark parent classes of nested test classes as in use
    //
    // So, we get this class. Any class marked with UsageInspectionsSupressorAttribute is instantiated
    // as part of the usage checking daemon process. It allows us to suppress the warning for any given
    // element. So, we do the decent thing and ask the UnitTestManager if any unit test provider considers
    // the requested element as being a "test element". This means IsUnitTestElement is expected to return
    // true for elements that strictly aren't directly unit test methods or classes - equivalent to the
    // (amusingly named) IsUnitTestStuff from previous versions of ReSharper.
    //
    // Arguably, this class should be implemented by ReSharper.
    [UsageInspectionsSupressor]
    public class SuppressUnusedUnitTestElements : IUsageInspectionsSupressor
    {
        public bool SupressUsageInspectionsOnElement(IDeclaredElement element)
        {
            return UnitTestManager.GetInstance(SolutionManager.Instance.CurrentSolution).IsUnitTestElement(element);
        }
    }
}
