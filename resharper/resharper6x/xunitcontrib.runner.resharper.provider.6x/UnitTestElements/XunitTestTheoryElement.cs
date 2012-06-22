using System.Collections.Generic;
using System.Xml;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.Util;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class XunitTestTheoryElement : IUnitTestElement, ISerializableUnitTestElement
    {
        private readonly ProjectModelElementEnvoy projectModelElementEnvoy;
        private XunitTestMethodElement parent;
        private UnitTestElementState state;

        public XunitTestTheoryElement(IUnitTestProvider provider, XunitTestMethodElement testMethod, ProjectModelElementEnvoy projectModelElementEnvoy, string id)
        {
            Provider = provider;
            Parent = testMethod;
            this.projectModelElementEnvoy = projectModelElementEnvoy;
            Id = id;
            State = UnitTestElementState.Dynamic;
        }

        public IProject GetProject()
        {
            return projectModelElementEnvoy.GetValidProjectElement() as IProject;
        }

        public string GetPresentation()
        {
            var name = parent.TypeName.FullName + ".";
            if (Id.StartsWith(name))
                return Id.Substring(name.Length);
            return Id;
        }

        public UnitTestNamespace GetNamespace()
        {
            return Parent == null ? null : Parent.GetNamespace();
        }

        public UnitTestElementDisposition GetDisposition()
        {
            return Parent == null ? null : Parent.GetDisposition();
        }

        public IDeclaredElement GetDeclaredElement()
        {
            return Parent == null ? null : Parent.GetDeclaredElement();
        }

        public IEnumerable<IProjectFile> GetProjectFiles()
        {
            return Parent == null ? null : Parent.GetProjectFiles();
        }

        public IList<UnitTestTask> GetTaskSequence(IList<IUnitTestElement> explicitElements)
        {
            return EmptyList<UnitTestTask>.InstanceList;
        }

        public string Kind
        {
            get { return "xUnit.net Theory"; }
        }

        public IEnumerable<UnitTestElementCategory> Categories
        {
            get { return UnitTestElementCategory.Uncategorized; }
        }

        public string ExplicitReason { get; private set; }
        public string Id { get; private set; }
        public IUnitTestProvider Provider { get; private set; }
        public IUnitTestElement Parent
        {
            get { return parent; }
            set
            {
                if (parent == value)
                    return;

                if (parent != null)
                    parent.RemoveChild(this);
                parent = (XunitTestMethodElement) value;
                if (parent != null)
                    parent.AddChild(this);
            }
        }

        public ICollection<IUnitTestElement> Children
        {
            get { return EmptyArray<IUnitTestElement>.Instance; }
        }

        public string ShortName
        {
            get { return Id; }
        }

        public bool Explicit
        {
            get { return false; }
        }

        public UnitTestElementState State
        {
            get { return state; }
            set { state = value == UnitTestElementState.Valid ? UnitTestElementState.Dynamic : value; }
        }

        public void WriteToXml(XmlElement element)
        {
        }

        public bool Equals(IUnitTestElement other)
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