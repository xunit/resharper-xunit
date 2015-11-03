using System;
using System.Collections.Generic;
using System.Xml;
using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.UnitTestFramework;
using XunitContrib.Runner.ReSharper.RemoteRunner.Tasks;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class XunitTestTheoryElement : XunitBaseElement, ISerializableUnitTestElement, IEquatable<XunitTestTheoryElement>
    {
        public XunitTestTheoryElement(XunitServiceProvider services, UnitTestElementId id,
                                      IClrTypeName typeName, string shortName)
            : base(services, id, typeName)
        {
            ShortName = shortName;
        }

        public override string GetPresentation(IUnitTestElement parentElement, bool full)
        {
            return full ? Id.Id : ShortName;
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

        public override IList<UnitTestTask> GetTaskSequence(ICollection<IUnitTestElement> explicitElements, IUnitTestRun run)
        {
            var sequence = Parent.GetTaskSequence(explicitElements, run);
            var methodTask = sequence[sequence.Count - 1].RemoteTask as XunitTestMethodTask;
            var theoryTask = new XunitTestTheoryTask(methodTask, ShortName);
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

        internal static IUnitTestElement ReadFromXml(XmlElement parent, IUnitTestElement parentElement, IProject project,
                                                     string id, UnitTestElementFactory unitTestElementFactory)
        {
            var methodElement = parentElement as XunitTestMethodElement;
            if (methodElement == null)
                throw new InvalidOperationException("parentElement should be xUnit.net test method");

            var name = parent.GetAttribute("name");

            return unitTestElementFactory.GetOrCreateTestTheory(id, project, methodElement, name);
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