using System;
using System.Xml;
using JetBrains.Annotations;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tasks
{
    [Serializable]
    public class XunitTestTheoryTask : DynamicElementXunitTaskBase, IEquatable<XunitTestTheoryTask>
    {
        public XunitTestTheoryTask(XunitTestMethodTask methodTask, string theoryName)
            : this(methodTask.Id, methodTask.ProjectId, methodTask.TypeName, methodTask.MethodName, theoryName)
        {
        }

        public XunitTestTheoryTask(string parentTaskId, string projectId, string typeName, string methodName, string theoryName)
            : base(parentTaskId)
        {
            ProjectId = projectId;
            TypeName = typeName;
            MethodName = methodName;
            TheoryName = theoryName;
        }

        [UsedImplicitly]
        public XunitTestTheoryTask(XmlElement element)
            : base(element)
        {
            ProjectId = GetXmlAttribute(element, AttributeNames.ProjectId);
            TypeName = GetXmlAttribute(element, AttributeNames.TypeName);
            MethodName = GetXmlAttribute(element, AttributeNames.MethodName);
            TheoryName = GetXmlAttribute(element, AttributeNames.TheoryName);
        }

        public override bool IsMeaningfulTask
        {
            get { return true; }
        }

        public string ProjectId { get; private set; }
        public string TypeName { get; private set; }
        public string MethodName { get; private set; }
        public string TheoryName { get; private set; }

        public override void SaveXml(XmlElement element)
        {
            base.SaveXml(element);
            SetXmlAttribute(element, AttributeNames.ProjectId, ProjectId);
            SetXmlAttribute(element, AttributeNames.TypeName, TypeName);
            SetXmlAttribute(element, AttributeNames.MethodName, MethodName);
            SetXmlAttribute(element, AttributeNames.TheoryName, TheoryName);
        }

        public bool Equals(XunitTestTheoryTask other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            // Do not include base.Equals, so we don't try to compare Id or ParentId,
            // which will be different for each instance, and we're trying to act like
            // a value type
            return ProjectId.Equals(other.ProjectId)
                && TypeName.Equals(other.TypeName)
                && MethodName.Equals(other.MethodName)
                && TheoryName.Equals(other.TheoryName);
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
                var result = ProjectId.GetHashCode();
                result = (result*397) ^ TypeName.GetHashCode();
                result = (result*397) ^ MethodName.GetHashCode();
                result = (result*397) ^ TheoryName.GetHashCode();
                return result;
            }
        }

        public override string ToString()
        {
            return string.Format("XunitTestTheoryTask<{0}>[{1}.{2}:{3}]", Id, TypeName, MethodName, TheoryName);
        }
    }
}