using System;
using System.Xml;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    [Serializable]
    public class XunitTestAssemblyTask : RemoteTask, IEquatable<XunitTestAssemblyTask>
    {
        private readonly string assemblyLocation;

        public XunitTestAssemblyTask(string assemblyLocation) : base(XunitTaskRunner.RunnerId)
        {
            this.assemblyLocation = assemblyLocation;
        }

        // This constructor is used to rehydrate a task from an xml element. This is
        // used by the remote test runner's IsolatedAssemblyTestRunner, which creates
        // an app domain to isolate the test assembly from the remote process framework.
        // That framework retrieves these tasks from devenv/resharper via remoting (hence
        // the SerializableAttribute) but uses this hand rolled xml serialisation to
        // get the tasks into the app domain that will actually run the tests
        public XunitTestAssemblyTask(XmlElement element) : base(element)
        {
            assemblyLocation = GetXmlAttribute(element, AttributeNames.AssemblyLocation);
        }

        public string AssemblyLocation
        {
            get { return assemblyLocation; }
        }

        public override void SaveXml(XmlElement element)
        {
            base.SaveXml(element);
            SetXmlAttribute(element, AttributeNames.AssemblyLocation, assemblyLocation);
        }

        public bool Equals(XunitTestAssemblyTask otherAssemblyTask)
        {
            if (otherAssemblyTask == null || !base.Equals(otherAssemblyTask))
                return false;

            return Equals(assemblyLocation, otherAssemblyTask.assemblyLocation);
        }

        public override bool Equals(object obj)
        {
            return this == obj || Equals(obj as XunitTestAssemblyTask);
        }

        // Blimey. ReSharper created this
        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (assemblyLocation != null ? assemblyLocation.GetHashCode() : 0);
            }
        }
    }
}
