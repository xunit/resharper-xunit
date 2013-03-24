using System;
using System.Collections.Generic;
using System.Xml;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.Util;
using XunitContrib.Runner.ReSharper.RemoteRunner;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class XunitTestTheoryElement : XunitBaseElement, IUnitTestElement, ISerializableUnitTestElement, IEquatable<XunitTestTheoryElement>
    {
        private readonly ProjectModelElementEnvoy projectModelElementEnvoy;
        private XunitTestMethodElement parent;
        private UnitTestElementState state;

        public XunitTestTheoryElement(IUnitTestProvider provider, XunitTestMethodElement testMethod, ProjectModelElementEnvoy projectModelElementEnvoy, string id, string shortName)
        {
            Provider = provider;
            Parent = testMethod;
            this.projectModelElementEnvoy = projectModelElementEnvoy;
            Id = id;
            State = UnitTestElementState.Dynamic;
            ShortName = shortName;
            ExplicitReason = string.Empty;
        }

        public IProject GetProject()
        {
            return projectModelElementEnvoy.GetValidProjectElement() as IProject;
        }

        // ReSharper 6.1
        public string GetPresentation()
        {
            return GetPresentation(null);
        }

        // ReSharper 7.0
        public string GetPresentation(IUnitTestElement parentElement)
        {
            return ShortName;
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

        // ReSharper 6.1
        public IList<UnitTestTask> GetTaskSequence(IList<IUnitTestElement> explicitElements)
        {
            return GetTaskSequence(explicitElements, null);
        }

        // ReSharper 7.0
        public IList<UnitTestTask> GetTaskSequence(ICollection<IUnitTestElement> explicitElements, IUnitTestLaunch launch)
        {
            var sequence = parent.GetTaskSequence(explicitElements, launch);
            sequence.Add(new UnitTestTask(this, new XunitTestTheoryTask(parent.Id, ShortName)));
            return sequence;
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

        public string ShortName { get; private set; }
        public bool Explicit { get { return false; } }

        public UnitTestElementState State
        {
            get { return state; }
            set { state = value == UnitTestElementState.Valid ? UnitTestElementState.Dynamic : value; }
        }

        public void WriteToXml(XmlElement element)
        {
            element.SetAttribute("projectId", GetProject().GetPersistentID());
            element.SetAttribute("name", ShortName);
            element.SetAttribute("parentElementId", parent.Id);
        }

        internal static IUnitTestElement ReadFromXml(XmlElement parent, IUnitTestElement parentElement, ISolution solution, UnitTestElementFactory unitTestElementFactory)
        {
            var methodElement = parentElement as XunitTestMethodElement;
            if (methodElement == null)
                throw new InvalidOperationException("parentElement should be xUnit.net test method");

            var projectId = parent.GetAttribute("projectId");
            var name = parent.GetAttribute("name");

            var project = (IProject)ProjectUtil.FindProjectElementByPersistentID(solution, projectId);
            if (project == null)
                return null;

            return unitTestElementFactory.GetOrCreateTestTheory(project, methodElement, name);
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