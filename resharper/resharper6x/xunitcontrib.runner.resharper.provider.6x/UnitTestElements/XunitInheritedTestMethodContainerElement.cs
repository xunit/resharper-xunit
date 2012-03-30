using System;
using System.Collections.Generic;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.Util;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class XunitInheritedTestMethodContainerElement : IUnitTestElement
    {
        private readonly IProject project;
        private readonly string methodName;

        public XunitInheritedTestMethodContainerElement(IUnitTestProvider provider, IProject project, string id, IClrTypeName typeName, string methodName)
        {
            Provider = provider;
            this.project = project;
            TypeName = typeName;
            this.methodName = methodName;
            Id = id;
            State = UnitTestElementState.Fake;
            ExplicitReason = null;
        }

        public IClrTypeName TypeName { get; private set; }

        public string Id { get; private set; }
        public string ExplicitReason { get; private set; }
        public IUnitTestProvider Provider { get; private set; }
        public UnitTestElementState State { get; set; }

        public IProject GetProject()
        {
            return project;
        }

        public string GetPresentation()
        {
            return methodName;
        }

        public UnitTestNamespace GetNamespace()
        {
            return new UnitTestNamespace(TypeName.GetNamespaceName());
        }

        public UnitTestElementDisposition GetDisposition()
        {
            return UnitTestElementDisposition.InvalidDisposition;
        }

        public IDeclaredElement GetDeclaredElement()
        {
            return null;
        }

        public IEnumerable<IProjectFile> GetProjectFiles()
        {
            throw new InvalidOperationException("Test from abstract fixture should not appear in Unit Test Explorer");
        }

        public IList<UnitTestTask> GetTaskSequence(IEnumerable<IUnitTestElement> explicitElements)
        {
            throw new InvalidOperationException("Test from abstract fixture is not runnable itself");
        }

        public IList<UnitTestTask> GetTaskSequence(IList<IUnitTestElement> explicitElements)
        {
            throw new InvalidOperationException("Test from abstract fixture is not runnable itself");
        }

        public string Kind
        {
            get { return "xUnit.net Test"; }
        }

        public IEnumerable<UnitTestElementCategory> Categories
        {
            get { return UnitTestElementCategory.Uncategorized; }
        }

        public IUnitTestElement Parent
        {
            get { return null; }
            set {  }
        }

        public ICollection<IUnitTestElement> Children
        {
            get { return EmptyList<IUnitTestElement>.InstanceList; }
        }

        public string ShortName
        {
            get { return methodName; }
        }

        public bool Explicit
        {
            get { return false; }
        }

        public bool Equals(IUnitTestElement other)
        {
            throw new NotImplementedException();
        }
    }
}