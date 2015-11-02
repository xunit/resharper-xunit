using JetBrains.ProjectModel;
using JetBrains.ReSharper.FeaturesTestFramework.Completion;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;
using XunitContrib.Runner.ReSharper.UnitTestProvider.PropertyData;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.References
{
    [Xunit1TestReferences]
    [TestFileExtension(VBProjectFileType.VB_EXTENSION)]
    [IncludeMsCorLib]
    public class VBPropertyDataTests : ReferenceTestBase
    {
        // How does ReferenceTestBase work?
        // Override RelativeTestDataPath to tell it where the data + gold files are
        // Override Format to provide formatting for reference targets that aren't paths, modules or declared elements
        // Override PresentExtraInfo to provide more details about an IReference
        // Implement AcceptReference to allow filtering of references to output
        // Override BeforeTestStarted + AfterTestFinished for anything else
        // Uses ReferenceTestUtil.ReferenceCollector to collect references + sort
        // Uses TestFrameworkUtil.DumpReferencePositions to add references into original text, e.g. |System|(0).Xml
        // Loops over all references, resolves and outputs result + reference target (using Format)
        // also calls PresentExtraInfo + dumps candidate info

        protected override string RelativeTestDataPath { get { return @"References\"; } }

        protected override bool AcceptReference(IReference reference)
        {
            return reference is PropertyDataReference;
        }

        [Test] public void PropertyDataInSameClass() { DoNamedTest(); }
        [Test] public void PropertyDataInOtherClass() { DoNamedTest(); }
        [Test] public void PropertyDataFromDerivedClass() { DoNamedTest(); }
        [Test] public void InvalidPropertyDataProperties() { DoNamedTest(); }
        [Test] public void UnresolvedPropertyData() { DoNamedTest(); }

        protected override string Format(IDeclaredElement declaredElement, ISubstitution substitution, PsiLanguageType languageType, DeclaredElementPresenterStyle presenter, IReference reference)
        {
            var format = base.Format(declaredElement, substitution, languageType, presenter, reference);
            if (declaredElement != null)
                format += " (" + declaredElement.GetElementType().PresentableName + ")";
            return format;
        }
    }

    [Xunit1TestReferences]
    [TestFileExtension(VBProjectFileType.VB_EXTENSION)]
    public class VBPropertyDataCompletionTests : VBCodeCompletionTestBase
    {
        protected override string RelativeTestDataPath { get { return @"References\CodeCompletion\"; } }

        [Test] public void ListsPropertyDataCandidatesInSameClass() { DoNamedTest(); }
        [Test] public void ListsPropertyDataCandidatesInOtherClass() { DoNamedTest(); }
    }
}