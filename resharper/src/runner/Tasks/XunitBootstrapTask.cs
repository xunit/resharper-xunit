using System;
using System.Globalization;
using System.Xml;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tasks
{
    // This class serves two purposes. Firstly, it notifies the runner of some
    // config that's only available from the provider (i.e. if the host is running,
    // debugging, covering or continuous testing). Secondly, it means that the
    // first task in the sequence isn't XunitTestAssemblyTask, which contains
    // the path of the assembly under test. This greatly helps the tests for
    // the runner itself, as the first task is output to the gold file, and
    // showing the path to the assembly under test will break things if we
    // run the tests in a different location. We can serialise this task,
    // as the project ID is constant in test runs, and there is nothing
    // else changeable in it. As long as we never report the assembly
    // task (there's no need, it's not a meaningful task - not associated
    // with an element), then our tests will pass.
    [Serializable]
    public class XunitBootstrapTask : RemoteTask, IEquatable<XunitBootstrapTask>
    {

        public XunitBootstrapTask(string projectId, bool disableAllConcurrency)
            : base(XunitTaskRunner.RunnerId)
        {
            DisableAllConcurrency = disableAllConcurrency;
            ProjectId = projectId;
        }

        public XunitBootstrapTask(XmlElement element)
            : base(element)
        {
            ProjectId = GetXmlAttribute(element, AttributeNames.ProjectId);

            bool disableAllConcurrency;
            if (!bool.TryParse(GetXmlAttribute(element, AttributeNames.DisableAllConcurrency), out disableAllConcurrency))
                disableAllConcurrency = false;
            DisableAllConcurrency = disableAllConcurrency;
        }

        public string ProjectId { get; set; }
        public bool DisableAllConcurrency { get; set; }

        public override bool IsMeaningfulTask { get { return false; }}

        public override void SaveXml(XmlElement element)
        {
            base.SaveXml(element);
            SetXmlAttribute(element, AttributeNames.ProjectId, ProjectId);
            SetXmlAttribute(element, AttributeNames.DisableAllConcurrency, DisableAllConcurrency.ToString(CultureInfo.InvariantCulture));
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