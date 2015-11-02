using JetBrains.ReSharper.FeaturesTestFramework.Completion;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;
using XunitContrib.Runner.ReSharper.UnitTestProvider.PropertyData;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.References
{
    [Xunit2TestReferences]
    [IncludeMsCorLib]
    public class CSharpMemberDataTests : ReferenceTestBase
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
    }

    [Xunit2TestReferences]
    public class CSharpMemberDataCompletionTests : CodeCompletionTestBase
    {
        protected override string RelativeTestDataPath { get { return @"References\CodeCompletion\"; } }

        protected override bool CheckAutomaticCompletionDefault()
        {
            return true;
        }

        protected override CodeCompletionTestType TestType
        {
            get { return CodeCompletionTestType.List; }
        }

        [Test] public void ListsMemberDataCandidatesInSameClass() { DoNamedTest(); }
        [Test] public void ListsMemberDataCandidatesInOtherClass() { DoNamedTest(); }
    }
}