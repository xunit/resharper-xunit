using System;
using System.Xml;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    [Serializable]
    public class XunitTestTheoryTask : RemoteTask, IEquatable<XunitTestTheoryTask>
    {
        public XunitTestTheoryTask(string parentElementId, string name)
            : base(XunitTaskRunner.RunnerId)
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

            // Don't include base.Equals, as RemoteTask.Equals includes RemoteTask.Id
            // in the calculation, and this is a new guid generated for each new instance
            // Using RemoteTask.Id in the Equals means collapsing the return values of
            // IUnitTestElement.GetTaskSequence into a tree will fail (as no assembly,
            // or class tasks will return true from Equals)
            return ParentElementId.Equals(other.ParentElementId) && Equals(other.Name, Name);
        }

        public override bool Equals(RemoteTask other)
        {
            return Equals(other as XunitTestTheoryTask);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as XunitTestTheoryTask);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // Don't include base.GetHashCode, as RemoteTask.GetHashCode includes RemoteTask.Id
                // in the calculation, and this is a new guid generated for each new instance.
                // This would mean two instances that return true from Equals (i.e. value objects)
                // would have different hash codes
                var result = Name.GetHashCode();
                result = (result*397) ^ ParentElementId.GetHashCode();
                return result;
            }
        }

        public override string ToString()
        {
            return string.Format("XunitTestTheoryTask<{0}>[{1}, {2}]", Id, ParentElementId, Name);
        }
    }
}