using System;
using System.Xml;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    [Serializable]
    public class XunitTestClassTask : RemoteTask, IEquatable<XunitTestClassTask>
    {
        private readonly string typeName;
        private readonly bool explicitly;
        // We don't use assemblyLocation, but we want to keep it so that if we run all the assemblies
        // in a solution, we are guaranteed that assembly + typeName will be unique. TypeName by itself
        // might not be. And if we have duplicate tasks, then some tests won't run. Pathological edge
        // case discovered by the manual tests reusing a whole bunch of code...
        private readonly string assemblyLocation;

        public XunitTestClassTask(string assemblyLocation, string typeName, bool explicitly) : base(XunitTestRunner.RunnerId)
        {
            if (assemblyLocation == null)
                throw new ArgumentNullException("assemblyLocation");
            if (typeName == null)
                throw new ArgumentNullException("typeName");

            this.assemblyLocation = assemblyLocation;
            this.typeName = typeName;
            this.explicitly = explicitly;
        }

        // This constructor is used to rehydrate a task from an xml element. This is
        // used by the remote test runner's IsolatedAssemblyTestRunner, which creates
        // an app domain to isolate the test assembly from the remote process framework.
        // That framework retrieves these tasks from devenv/resharper via remoting (hence
        // the SerializableAttribute) but uses this hand rolled xml serialisation to
        // get the tasks into the app domain that will actually run the tests
        public XunitTestClassTask(XmlElement element) : base(element)
        {
            assemblyLocation = GetXmlAttribute(element, AttributeNames.AssemblyLocation);
            typeName = GetXmlAttribute(element, AttributeNames.TypeName);
            explicitly = bool.Parse(GetXmlAttribute(element, AttributeNames.Explicitly));
        }

        public string TypeName
        {
            get { return typeName; }
        }

        public override void SaveXml(XmlElement element)
        {
            base.SaveXml(element);
            SetXmlAttribute(element, AttributeNames.AssemblyLocation, assemblyLocation);
            SetXmlAttribute(element, AttributeNames.TypeName, typeName);
            SetXmlAttribute(element, AttributeNames.Explicitly, explicitly.ToString());
        }

        public bool Equals(XunitTestClassTask otherClassTask)
        {
            if (otherClassTask == null)
                return false;

            return (Equals(assemblyLocation, otherClassTask.assemblyLocation) 
                && Equals(typeName, otherClassTask.typeName) 
                && explicitly == otherClassTask.explicitly);
        }

        public override bool Equals(RemoteTask other)
        {
            return Equals(other as XunitTestClassTask);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ (assemblyLocation != null ? assemblyLocation.GetHashCode() : 0);
                result = (result * 397) ^ (typeName != null ? typeName.GetHashCode() : 0);
                result = (result * 397) ^ explicitly.GetHashCode();
                return result;
            }
        }

        public override bool IsMeaningfulTask
        {
            get { return true; }
        }
    }
}