using System;
using System.Collections.Generic;
using System.Xml;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.UnitTestFramework;
using XunitContrib.Runner.ReSharper.RemoteRunner;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class XunitTestTheoryElement : XunitBaseElement, ISerializableUnitTestElement, IEquatable<XunitTestTheoryElement>
    {
        public XunitTestTheoryElement(IUnitTestProvider provider, XunitTestMethodElement methodElement, 
                                      ProjectModelElementEnvoy projectModelElementEnvoy, string id,
                                      string shortName)
            : base(provider, methodElement, id, projectModelElementEnvoy, new JetHashSet<string>())
        {
            SetState(UnitTestElementState.Dynamic);
            ShortName = shortName;
            ExplicitReason = string.Empty;
        }

        private XunitTestMethodElement MethodElement
        {
            get { return Parent as XunitTestMethodElement; }
        }

        public override string GetPresentation(IUnitTestElement parentElement)
        {
            return ShortName;
        }

        public override UnitTestNamespace GetNamespace()
        {
            return Parent == null ? null : Parent.GetNamespace();
        }

        public override UnitTestElementDisposition GetDisposition()
        {
            return Parent == null ? null : Parent.GetDisposition();
        }

        public override IDeclaredElement GetDeclaredElement()
        {
            return Parent == null ? null : Parent.GetDeclaredElement();
        }

        public override IEnumerable<IProjectFile> GetProjectFiles()
        {
            return Parent == null ? null : Parent.GetProjectFiles();
        }

        public override IList<UnitTestTask> GetTaskSequence(ICollection<IUnitTestElement> explicitElements, IUnitTestLaunch launch)
        {
            var sequence = ((XunitBaseElement) Parent).GetTaskSequence(explicitElements, launch);
            var theoryTask = new XunitTestTheoryTask(MethodElement.AssemblyLocation, MethodElement.TypeName.FullName, MethodElement.MethodName, ShortName);
            sequence.Add(new UnitTestTask(this, theoryTask));
            return sequence;
        }

        public override string Kind
        {
            get { return "xUnit.net Theory"; }
        }

        public override UnitTestElementState State
        {
            get { return base.State; }
            set { base.State = (value == UnitTestElementState.Valid) ? UnitTestElementState.Dynamic : value; }
        }

        public void WriteToXml(XmlElement element)
        {
            element.SetAttribute("name", ShortName);
        }

        internal static IUnitTestElement ReadFromXml(XmlElement parent, IUnitTestElement parentElement, ISolution solution, UnitTestElementFactory unitTestElementFactory)
        {
            var methodElement = parentElement as XunitTestMethodElement;
            if (methodElement == null)
                throw new InvalidOperationException("parentElement should be xUnit.net test method");

            var name = parent.GetAttribute("name");

            var project = methodElement.GetProject();
            if (project == null)
                return null;

            return unitTestElementFactory.GetOrCreateTestTheory(project, methodElement, name);
        }

        public override bool Equals(IUnitTestElement other)
        {
            return Equals(other as XunitTestTheoryElement);
        }

        public bool Equals(XunitTestTheoryElement other)
        {
            if (other == null)
                return false;

            return Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(XunitTestTheoryElement)) return false;
            return Equals((XunitTestTheoryElement) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}