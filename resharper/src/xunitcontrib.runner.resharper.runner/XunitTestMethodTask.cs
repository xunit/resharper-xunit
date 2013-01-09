using System;
using System.Xml;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    [Serializable]
    public class XunitTestMethodTask : RemoteTask, IEquatable<XunitTestMethodTask>
    {
        private readonly bool explicitly;
        // We don't use assemblyLocation, but we want to keep it so that if we run all the assemblies
        // in a solution, we are guaranteed that assembly + typeName will be unique. TypeName by itself
        // might not be. And if we have duplicate tasks, then some tests won't run. Pathological edge
        // case discovered by the manual tests reusing a whole bunch of code...
        private readonly string assemblyLocation;

        public XunitTestMethodTask(string id, string assemblyLocation, string classTypeName, string methodName, bool explicitly)
            : base(XunitTaskRunner.RunnerId)
        {
            if (assemblyLocation == null)
                throw new ArgumentNullException("assemblyLocation");
            if (methodName == null)
                throw new ArgumentNullException("methodName");
            if (classTypeName == null)
                throw new ArgumentNullException("classTypeName");

            ElementId = id;
            this.assemblyLocation = assemblyLocation;
            TypeName = classTypeName;
            MethodName = methodName;
            this.explicitly = explicitly;
        }

        // This constructor is used to rehydrate a task from an xml element. This is
        // used by the remote test runner's IsolatedAssemblyTestRunner, which creates
        // an app domain to isolate the test assembly from the remote process framework.
        // That framework retrieves these tasks from devenv/resharper via remoting (hence
        // the SerializableAttribute) but uses this hand rolled xml serialisation to
        // get the tasks into the app domain that will actually run the tests
        public XunitTestMethodTask(XmlElement element)
            : base(element)
        {
            ElementId = GetXmlAttribute(element, AttributeNames.ElementId);
            assemblyLocation = GetXmlAttribute(element, AttributeNames.AssemblyLocation);
            TypeName = GetXmlAttribute(element, AttributeNames.TypeName);
            MethodName = GetXmlAttribute(element, AttributeNames.MethodName);
            explicitly = bool.Parse(GetXmlAttribute(element, AttributeNames.Explicitly));
        }

        public string TypeName { get; private set; }
        public string MethodName { get; private set; }
        public string ElementId { get; private set; }

        public override bool IsMeaningfulTask
        {
            get { return true; }
        }

        public override void SaveXml(XmlElement element)
        {
            base.SaveXml(element);
            SetXmlAttribute(element, AttributeNames.ElementId, ElementId);
            SetXmlAttribute(element, AttributeNames.AssemblyLocation, assemblyLocation);
            SetXmlAttribute(element, AttributeNames.TypeName, TypeName);
            SetXmlAttribute(element, AttributeNames.MethodName, MethodName);
            SetXmlAttribute(element, AttributeNames.Explicitly, explicitly.ToString());
        }

        public bool Equals(XunitTestMethodTask other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            // Don't include base.Equals, as RemoteTask.Equals includes RemoteTask.Id
            // in the calculation, and this is a new guid generated for each new instance
            // Using RemoteTask.Id in the Equals means collapsing the return values of
            // IUnitTestElement.GetTaskSequence into a tree will fail (as no assembly,
            // or class tasks will return true from Equals)
            return Equals(ElementId, other.ElementId) &&
                   Equals(assemblyLocation, other.assemblyLocation) &&
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
                // Don't include base.GetHashCode, as RemoteTask.GetHashCode includes RemoteTask.Id
                // in the calculation, and this is a new guid generated for each new instance.
                // This would mean two instances that return true from Equals (i.e. value objects)
                // would have different hash codes
                int result = ElementId.GetHashCode();
                result = (result*397) ^ explicitly.GetHashCode();
                result = (result*397) ^ (TypeName != null ? TypeName.GetHashCode() : 0);
                result = (result*397) ^ (MethodName != null ? MethodName.GetHashCode() : 0);
                result = (result*397) ^ (assemblyLocation != null ? assemblyLocation.GetHashCode() : 0);
                return result;
            }
        }

        public override string ToString()
        {
            return string.Format("XunitTestMethodTask[{0}]({1}.{2})", ElementId, TypeName, MethodName);
        }
    }
}