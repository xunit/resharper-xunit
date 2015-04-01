using System;
using System.Xml;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tasks
{
    // This class's only purpose is to be the first class in a task sequence.
    // The first task is always serialised and sent from the external process
    // to the main ReSharper process. If we use XunitTestAssemblyTask as the
    // first class, it includes the path to the assembly under test, which
    // means failing tests if the assembly is in a different location to what's
    // in the gold files. So, this class is used first, and when it gets
    // serialised, there is nothing changeable in it (the project id is constant
    // for tests) and as long as we never report the assembly task (there's no
    // need, it's not a meaningful task - not associated with an element), then
    // our tests will pass
    [Serializable]
    public class XunitBootstrapTask : RemoteTask, IEquatable<XunitBootstrapTask>
    {
        public XunitBootstrapTask(string projectId)
            : base(XunitTaskRunner.RunnerId)
        {
            ProjectId = projectId;
        }

        public XunitBootstrapTask(XmlElement element)
            : base(element)
        {
            ProjectId = GetXmlAttribute(element, AttributeNames.ProjectId);
        }

        public string ProjectId { get; set; }
        public override bool IsMeaningfulTask { get { return false; }}

        public override void SaveXml(XmlElement element)
        {
            base.SaveXml(element);
            SetXmlAttribute(element, AttributeNames.ProjectId, ProjectId);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as XunitBootstrapTask);
        }

        public override bool Equals(RemoteTask remoteTask)
        {
            return Equals(remoteTask as XunitBootstrapTask);
        }

        public bool Equals(XunitBootstrapTask other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(ProjectId, other.ProjectId);
        }

        public override int GetHashCode()
        {
            return ProjectId != null ? ProjectId.GetHashCode() : 0;
        }
    }
}