using System;
using System.Xml;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    [Serializable]
    public class XunitTestTheoryTask : RemoteTask, IEquatable<XunitTestTheoryTask>
    {
        public XunitTestTheoryTask(string parentElementId, string name)
            : base(XunitTestRunner.RunnerId)
        {
            Name = name;
            ParentElementId = parentElementId;
        }

        public XunitTestTheoryTask(XmlElement element)
            : base(element)
        {
            Name = GetXmlAttribute(element, AttributeNames.Name);
            ParentElementId = GetXmlAttribute(element, AttributeNames.ParentElementId);
        }

        public override bool IsMeaningfulTask
        {
            get { return true; }
        }

        public string Name { get; private set; }
        public string ParentElementId { get; private set; }

        public override void SaveXml(XmlElement element)
        {
            base.SaveXml(element);
            SetXmlAttribute(element, AttributeNames.Name, Name);
            SetXmlAttribute(element, AttributeNames.ParentElementId, ParentElementId);
        }

        public bool Equals(XunitTestTheoryTask other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            // TODO: Don't include base.Equals, because that uses the id, and blah blah, see GetHashCode
            return Equals(other.Name, Name) && ParentElementId.Equals(other.ParentElementId);
        }

        public override bool Equals(RemoteTask other)
        {
            return Equals(other as XunitTestTheoryTask);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as XunitTestTheoryTask);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // TODO: Don't include base.GetHashCode, because that includes the Id
                // which is a new guid for each new instance (unless serialised) and
                // TaskProvider doesn't (yet) look at the Tasks returned from 
                // XunitTestTheoryElement.GetTaskSequence
                var result = Name.GetHashCode();
                result = (result*397) ^ ParentElementId.GetHashCode();
                return result;
            }
        }

        public override string ToString()
        {
            return string.Format("XunitTestTheoryTask[{0}, {1}]", ParentElementId, Name);
        }
    }
}