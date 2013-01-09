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

        public bool Equals(XunitTestAssemblyTask other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            // Don't include base.Equals, as RemoteTask.Equals includes RemoteTask.Id
            // in the calculation, and this is a new guid generated for each new instance
            // Using RemoteTask.Id in the Equals means collapsing the return values of
            // IUnitTestElement.GetTaskSequence into a tree will fail (as no assembly,
            // or class tasks will return true from Equals)
            return Equals(assemblyLocation, other.assemblyLocation);
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
                // Don't include base.GetHashCode, as RemoteTask.GetHashCode includes RemoteTask.Id
                // in the calculation, and this is a new guid generated for each new instance.
                // This would mean two instances that return true from Equals (i.e. value objects)
                // would have different hash codes
                return assemblyLocation != null ? assemblyLocation.GetHashCode() : 0;
            }
        }

        public override bool IsMeaningfulTask
        {
            get { return true; }
        }
    }
}
