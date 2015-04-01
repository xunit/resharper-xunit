using System;
using System.Xml;
using JetBrains.Annotations;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tasks
{
    [Serializable]
    public class XunitTestMethodTask : DynamicElementXunitTaskBase, IEquatable<XunitTestMethodTask>
    {
        private readonly bool explicitly;

        public XunitTestMethodTask(XunitTestClassTask classTask, string methodName, bool explicitly, bool isDynamic)
            : this(classTask.Id, classTask.ProjectId, classTask.TypeName, methodName, explicitly, isDynamic)
        {
        }

        public XunitTestMethodTask(string parentId, string projectId, string classTypeName, string methodName, bool explicitly, bool isDynamic)
            : base(parentId)
        {
            if (projectId == null)
                throw new ArgumentNullException("projectId");
            if (methodName == null)
                throw new ArgumentNullException("methodName");
            if (classTypeName == null)
                throw new ArgumentNullException("classTypeName");

            ProjectId = projectId;
            TypeName = classTypeName;
            MethodName = methodName;
            this.explicitly = explicitly;
            IsDynamic = isDynamic;
        }

        // This constructor is used to rehydrate a task from an xml element. This is
        // used by the remote test runner's IsolatedAssemblyTestRunner, which creates
        // an app domain to isolate the test assembly from the remote process framework.
        // That framework retrieves these tasks from devenv/resharper via remoting (hence
        // the SerializableAttribute) but uses this hand rolled xml serialisation to
        // get the tasks into the app domain that will actually run the tests
        [UsedImplicitly]
        public XunitTestMethodTask(XmlElement element)
            : base(element)
        {
            ProjectId = GetXmlAttribute(element, AttributeNames.ProjectId);
            TypeName = GetXmlAttribute(element, AttributeNames.TypeName);
            MethodName = GetXmlAttribute(element, AttributeNames.MethodName);
            explicitly = bool.Parse(GetXmlAttribute(element, AttributeNames.Explicitly));
            IsDynamic = bool.Parse(GetXmlAttribute(element, AttributeNames.Dynamic));
        }

        // See the comments in XunitTestClassTask.ProjectId
        public string ProjectId { get; private set; }
        public string TypeName { get; private set; }
        public string MethodName { get; private set; }
        public bool IsDynamic { get; private set; }

        public override bool IsMeaningfulTask
        {
            get { return true; }
        }

        public override void SaveXml(XmlElement element)
        {
            base.SaveXml(element);
            SetXmlAttribute(element, AttributeNames.ProjectId, ProjectId);
            SetXmlAttribute(element, AttributeNames.TypeName, TypeName);
            SetXmlAttribute(element, AttributeNames.MethodName, MethodName);
            SetXmlAttribute(element, AttributeNames.Explicitly, explicitly.ToString());
            SetXmlAttribute(element, AttributeNames.Dynamic, IsDynamic.ToString());
        }

        public bool Equals(XunitTestMethodTask other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            // Don't call base.Equals so that we don't try to compare Id or ParentId,
            // which are different per-instance and we're trying to behave like a value type
            return Equals(ProjectId, other.ProjectId) &&
                   Equals(MethodName, other.MethodName) &&
                   explicitly == other.explicitly;
        }

        public override bool Equals(RemoteTask other)
        {
            return Equals(other as XunitTestMethodTask);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as XunitTestMethodTask);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // Don't call base.GetHashCode so that we don't try to include Id or ParentId,
                // which are different per-instance and we're trying to behave like a value type
                int result = explicitly.GetHashCode();
                result = (result*397) ^ (TypeName != null ? TypeName.GetHashCode() : 0);
                result = (result*397) ^ (MethodName != null ? MethodName.GetHashCode() : 0);
                result = (result*397) ^ (ProjectId != null ? ProjectId.GetHashCode() : 0);
                return result;
            }
        }

        public override string ToString()
        {
            return string.Format("XunitTestMethodTask<{0}>({1}.{2})", Id, TypeName, MethodName);
        }
    }
}