using System;
using System.Xml;
using JetBrains.Annotations;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tasks
{
    [Serializable]
    public class XunitTestAssemblyTask : RemoteTask, IEquatable<XunitTestAssemblyTask>
    {
        public XunitTestAssemblyTask(string projectId, string assemblyLocation)
            : base(XunitTaskRunner.RunnerId)
        {
            ProjectId = projectId;
            AssemblyLocation = assemblyLocation;
        }

        // This constructor is used to rehydrate a task from an xml element. This is
        // used by the remote test runner's IsolatedAssemblyTestRunner, which creates
        // an app domain to isolate the test assembly from the remote process framework.
        // That framework retrieves these tasks from devenv/resharper via remoting (hence
        // the SerializableAttribute) but uses this hand rolled xml serialisation to
        // get the tasks into the app domain that will actually run the tests
        [UsedImplicitly]
        public XunitTestAssemblyTask(XmlElement element) : base(element)
        {
            ProjectId = GetXmlAttribute(element, AttributeNames.ProjectId);
            AssemblyLocation = GetXmlAttribute(element, AttributeNames.AssemblyLocation);
        }

        public string ProjectId { get; private set; }
        public string AssemblyLocation { get; private set; }

        public override void SaveXml(XmlElement element)
        {
            base.SaveXml(element);
            SetXmlAttribute(element, AttributeNames.ProjectId, ProjectId);
            SetXmlAttribute(element, AttributeNames.AssemblyLocation, AssemblyLocation);
        }

        public bool Equals(XunitTestAssemblyTask other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(ProjectId, other.ProjectId);
        }

        public override bool Equals(RemoteTask remoteTask)
        {
            return Equals(remoteTask as XunitTestAssemblyTask);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as XunitTestAssemblyTask);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ProjectId != null ? ProjectId.GetHashCode() : 0;
            }
        }

        public override bool IsMeaningfulTask
        {
            // This task doesn't correspond to an IUnitTestElement
            get { return false; }
        }
    }
}
