using System;
using System.Collections.Generic;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.Util;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class XunitInheritedTestMethodContainerElement : XunitBaseElement
    {
        private readonly string methodName;

        public XunitInheritedTestMethodContainerElement(IUnitTestProvider provider, ProjectModelElementEnvoy projectModelElementEnvoy, 
                                                        string id, IClrTypeName typeName, string methodName)
            : base(provider, null, id, projectModelElementEnvoy, EmptyArray<string>.Instance)
        {
            TypeName = typeName;
            this.methodName = methodName;
            ShortName = methodName;
            SetState(UnitTestElementState.Fake);
            ExplicitReason = null;
        }

        public IClrTypeName TypeName { get; private set; }

        public override string GetPresentation(IUnitTestElement parent)
        {
            return methodName;
        }

        public override UnitTestNamespace GetNamespace()
        {
            return new UnitTestNamespace(TypeName.GetNamespaceName());
        }

        public override UnitTestElementDisposition GetDisposition()
        {
            return UnitTestElementDisposition.InvalidDisposition;
        }

        public override IDeclaredElement GetDeclaredElement()
        {
            return null;
        }

        public override IEnumerable<IProjectFile> GetProjectFiles()
        {
            throw new InvalidOperationException("Test from abstract fixture should not appear in Unit Test Explorer");
        }

        public override IList<UnitTestTask> GetTaskSequence(ICollection<IUnitTestElement> explicitElements, IUnitTestLaunch launch)
        {
            throw new InvalidOperationException("Test from abstract fixture is not runnable itself");
        }

        public override string Kind
        {
            get { return "xUnit.net Test"; }
        }

        public override bool Equals(IUnitTestElement other)
        {
            throw new NotImplementedException();
        }
    }
}