using System;
using System.Globalization;
using System.Xml;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    [Serializable]
    public class XunitTestClassTask : RemoteTask, IEquatable<XunitTestClassTask>
    {
        public XunitTestClassTask(string assemblyLocation, string typeName, bool explicitly)
            : base(XunitTaskRunner.RunnerId)
        {
            if (assemblyLocation == null)
                throw new ArgumentNullException("assemblyLocation");
            if (typeName == null)
                throw new ArgumentNullException("typeName");

            AssemblyLocation = assemblyLocation;
            TypeName = typeName;
            Explicitly = explicitly;
        }

        // This constructor is used to rehydrate a task from an xml element. This is
        // used by the remote test runner's IsolatedAssemblyTestRunner, which creates
        // an app domain to isolate the test assembly from the remote process framework.
        // That framework retrieves these tasks from devenv/resharper via remoting (hence
        // the SerializableAttribute) but uses this hand rolled xml serialisation to
        // get the tasks into the app domain that will actually run the tests
        public XunitTestClassTask(XmlElement element) : base(element)
        {
            AssemblyLocation = GetXmlAttribute(element, AttributeNames.AssemblyLocation);
            TypeName = GetXmlAttribute(element, AttributeNames.TypeName);
            Explicitly = bool.Parse(GetXmlAttribute(element, AttributeNames.Explicitly));
        }

        // We don't use assemblyLocation, but we want to keep it so that if we run all the assemblies
        // in a solution, we are guaranteed that assembly + typeName will be unique. TypeName by itself
        // might not be. And if we have duplicate tasks, then some tests won't run. Pathological edge
        // case discovered by the manual tests reusing a whole bunch of code...
        public string AssemblyLocation { get; private set; }
        public string TypeName { get; private set; }
        public bool Explicitly { get; private set; }

        public override void SaveXml(XmlElement element)
        {
            base.SaveXml(element);
            SetXmlAttribute(element, AttributeNames.AssemblyLocation, AssemblyLocation);
            SetXmlAttribute(element, AttributeNames.TypeName, TypeName);
            SetXmlAttribute(element, AttributeNames.Explicitly, Explicitly.ToString(CultureInfo.InvariantCulture));
        }

        public bool Equals(XunitTestClassTask other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            // Don't include base.Equals, as RemoteTask.Equals includes RemoteTask.Id
            // in the calculation, and this is a new guid generated for each new instance
            // Using RemoteTask.Id in the Equals means collapsing the return values of
            // IUnitTestElement.GetTaskSequence into a tree will fail (as no assembly,
            // or class tasks will return true from Equals)
            return Equals(AssemblyLocation, other.AssemblyLocation) &&
                   Equals(TypeName, other.TypeName) &&
                   Explicitly == other.Explicitly;
        }

        public override bool Equals(RemoteTask other)
        {
            return Equals(other as XunitTestClassTask);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as XunitTestClassTask);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // Don't include base.GetHashCode, as RemoteTask.GetHashCode includes RemoteTask.Id
                // in the calculation, and this is a new guid generated for each new instance.
                // This would mean two instances that return true from Equals (i.e. value objects)
                // would have different hash codes
                int result = (AssemblyLocation != null ? AssemblyLocation.GetHashCode() : 0);
                result = (result * 397) ^ (TypeName != null ? TypeName.GetHashCode() : 0);
                result = (result * 397) ^ Explicitly.GetHashCode();
                return result;
            }
        }

        public override bool IsMeaningfulTask
        {
            get { return true; }
        }

        public override string ToString()
        {
            return string.Format("XunitTestClassTask<{0}>({1})", Id, TypeName);
        }
    }
}