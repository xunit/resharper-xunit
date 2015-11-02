using System;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.VB;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider.PropertyData
{
    [ReferenceProviderFactory]
    public class MemberDataReferenceProviderFactory : IReferenceProviderFactory
    {
        public IReferenceFactory CreateFactory(IPsiSourceFile sourceFile, IFile file)
        {
            if (sourceFile.PrimaryPsiLanguage.Is<CSharpLanguage>())
                return new CSharpMemberDataReferenceFactory();
            if (sourceFile.PrimaryPsiLanguage.Is<VBLanguage>())
                return new VBMemberDataReferenceFactory();
            return null;
        }

        public event Action OnChanged;
    }
}