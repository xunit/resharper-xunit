using System;
using System.Xml;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    [Serializable]
    public class XunitTestTheoryTask : RemoteTask
    {
        public XunitTestTheoryTask(string parentId, string name)
            : base(XunitTestRunner.RunnerId)
        {
            ParentId = parentId;
            Name = name;
        }

        public XunitTestTheoryTask(XmlElement element)
            : base(element)
        {
            Name = GetXmlAttribute(element, AttributeNames.Name);
            ParentId = GetXmlAttribute(element, AttributeNames.ParentId);
        }

        public override bool IsMeaningfulTask
        {
            get { return true; }
        }

        public string Name { get; private set; }
        public string ParentId { get; private set; }

        public override void SaveXml(XmlElement element)
        {
            base.SaveXml(element);
            SetXmlAttribute(element, AttributeNames.Name, Name);
            SetXmlAttribute(element, AttributeNames.ParentId, ParentId);
        }

        public bool Equals(XunitTestTheoryTask other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Equals(other.Name, Name) && ParentId.Equals(other.ParentId);
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
                int result = base.GetHashCode();
                result = (result*397) ^ Name.GetHashCode();
                result = (result*397) ^ ParentId.GetHashCode();
                return result;
            }
        }

        public override string ToString()
        {
            //return string.Format("XunitTestTheoryTask[{0}.{1}, {2}]", ParentId.TypeName, ParentId.ShortName, name);
            return string.Format("XunitTestTheoryTask[{0}, {1}]", ParentId, Name);
        }

        public static bool operator ==(XunitTestTheoryTask left, XunitTestTheoryTask right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(XunitTestTheoryTask left, XunitTestTheoryTask right)
        {
            return !Equals(left, right);
        }
    }
}