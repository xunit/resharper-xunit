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
    public class XunitTestTheoryElement : XunitBaseElement, ISerializableUnitTestElement, IEquatable<XunitTestTheoryElement>
    {
        private UnitTestElementState state;

        public XunitTestTheoryElement(IUnitTestProvider provider, XunitTestMethodElement testMethod, 
                                      ProjectModelElementEnvoy projectModelElementEnvoy, string id,
                                      string shortName)
            : base(provider, testMethod, id, projectModelElementEnvoy, EmptyArray<string>.Instance)
        {
            state = UnitTestElementState.Dynamic;
            ShortName = shortName;
            ExplicitReason = string.Empty;
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
            sequence.Add(new UnitTestTask(this, new XunitTestTheoryTask(Parent.Id, ShortName)));
            return sequence;
        }

        public override string Kind
        {
            get { return "xUnit.net Theory"; }
        }

        public override UnitTestElementState State
        {
            get { return state; }
            set { state = value == UnitTestElementState.Valid ? UnitTestElementState.Dynamic : value; }
        }

        public void WriteToXml(XmlElement element)
        {
            element.SetAttribute("projectId", GetProject().GetPersistentID());
            element.SetAttribute("name", ShortName);
            element.SetAttribute("parentElementId", Parent.Id);
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