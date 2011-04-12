using System;
using System.Xml;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    [Serializable]
    public class XunitTestMethodTask : RemoteTask, IEquatable<XunitTestMethodTask>
    {
        private readonly bool explicitly;
        private readonly string methodName;
        private readonly string classTypeName;
        // We don't use assemblyLocation, but we want to keep it so that if we run all the assemblies
        // in a solution, we are guaranteed that assembly + typeName will be unique. TypeName by itself
        // might not be. And if we have duplicate tasks, then some tests won't run. Pathological edge
        // case discovered by the manual tests reusing a whole bunch of code...
        private readonly string assemblyLocation;

        public XunitTestMethodTask(string assemblyLocation, string classTypeName, string methodName, bool explicitly)
            : base(XunitTaskRunner.RunnerId)
        {
            if (assemblyLocation == null)
                throw new ArgumentNullException("assemblyLocation");
            if (methodName == null)
                throw new ArgumentNullException("methodName");
            if (classTypeName == null)
                throw new ArgumentNullException("classTypeName");

            this.assemblyLocation = assemblyLocation;
            this.classTypeName = classTypeName;
            this.methodName = methodName;
            this.explicitly = explicitly;
        }

        // This constructor is used to rehydrate a task from an xml element. This is
        // used by the remote test runner's IsolatedAssemblyTestRunner, which creates
        // an app domain to isolate the test assembly from the remote process framework.
        // That framework retrieves these tasks from devenv/resharper via remoting (hence
        // the SerializableAttribute) but uses this hand rolled xml serialisation to
        // get the tasks into the app domain that will actually run the tests
        public XunitTestMethodTask(XmlElement element) : base(element)
        {
            assemblyLocation = GetXmlAttribute(element, AttributeNames.AssemblyLocation);
            classTypeName = GetXmlAttribute(element, AttributeNames.TypeName);
            methodName = GetXmlAttribute(element, AttributeNames.MethodName);
            explicitly = bool.Parse(GetXmlAttribute(element, AttributeNames.Explicitly));
        }

        public string TypeName
        {
            get { return classTypeName; }
        }

        public string ShortName
        {
            get { return methodName; }
        }

        public override void SaveXml(XmlElement element)
        {
            base.SaveXml(element);
            SetXmlAttribute(element, AttributeNames.AssemblyLocation, assemblyLocation);
            SetXmlAttribute(element, AttributeNames.TypeName, classTypeName);
            SetXmlAttribute(element, AttributeNames.MethodName, methodName);
            SetXmlAttribute(element, AttributeNames.Explicitly, explicitly.ToString());
        }

        public bool Equals(XunitTestMethodTask otherMethodTask)
        {
            if (otherMethodTask == null)
                return false;

            return Equals(assemblyLocation, otherMethodTask.assemblyLocation) && 
                   Equals(classTypeName, otherMethodTask.classTypeName) &&
                   Equals(methodName, otherMethodTask.methodName) &&
                   explicitly == otherMethodTask.explicitly;
        }

        public override bool Equals(RemoteTask other)
        {
            return Equals(other as XunitTestMethodTask);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result*397) ^ explicitly.GetHashCode();
                result = (result*397) ^ (methodName != null ? methodName.GetHashCode() : 0);
                result = (result*397) ^ (classTypeName != null ? classTypeName.GetHashCode() : 0);
                result = (result*397) ^ (assemblyLocation != null ? assemblyLocation.GetHashCode() : 0);
                return result;
            }
        }
    }
}