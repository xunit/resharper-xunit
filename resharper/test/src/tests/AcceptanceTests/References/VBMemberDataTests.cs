using JetBrains.ProjectModel;
using JetBrains.ReSharper.FeaturesTestFramework.Completion;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;
using XunitContrib.Runner.ReSharper.UnitTestProvider.PropertyData;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.References
{
    [Xunit2TestReferences]
    [TestFileExtension(VBProjectFileType.VB_EXTENSION)]
    [IncludeMsCorLib]
    public class VBMemberDataTests : ReferenceTestBase
    {
        protected override string RelativeTestDataPath { get { return @"References\"; } }

        protected override bool AcceptReference(IReference reference)
        {
            return reference is MemberDataReference;
        }

        [Test] public void MemberDataInSameClass() { DoNamedTest(); }
        [Test] public void MemberDataInOtherClass() { DoNamedTest(); }
        [Test] public void MemberDataFromDerivedClass() { DoNamedTest(); }
        [Test] public void InvalidMemberDataProperties() { DoNamedTest(); }
        [Test] public void UnresolvedMemberData() { DoNamedTest(); }

        protected override string Format(IDeclaredElement declaredElement, ISubstitution substitution, PsiLanguageType languageType, DeclaredElementPresenterStyle presenter, IReference reference)
        {
            var format = base.Format(declaredElement, substitution, languageType, presenter, reference);
            if (declaredElement != null)
                format += " (" + declaredElement.GetElementType().PresentableName + ")";
            return format;
        }
    }

    [Xunit2TestReferences]
    [TestFileExtension(VBProjectFileType.VB_EXTENSION)]
    public class VBMemberDataCompletionTests : VBCodeCompletionTestBase
    {
        protected override string RelativeTestDataPath { get { return @"References\CodeCompletion\"; } }

        [Test] public void ListsMemberDataCandidatesInSameClass() { DoNamedTest(); }
        [Test] public void ListsMemberDataCandidatesInOtherClass() { DoNamedTest(); }
    }
}