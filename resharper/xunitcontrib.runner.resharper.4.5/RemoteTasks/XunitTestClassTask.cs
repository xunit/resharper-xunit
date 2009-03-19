using System;
using System.Xml;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper
{
    [Serializable]
    public class XunitTestClassTask : RemoteTask, IEquatable<XunitTestClassTask>
    {
        readonly string assemblyLocation;

        [NonSerialized]
        ITestClassCommand command;

        readonly bool explicitly;
        readonly string typeName;

        public XunitTestClassTask(XmlElement element)
            : base(element)
        {
            typeName = GetXmlAttribute(element, "TypeName");
            assemblyLocation = GetXmlAttribute(element, "AssemblyLocation");
            explicitly = GetXmlAttribute(element, "Explicitly") == "true";
        }

        public XunitTestClassTask(string assemblyLocation,
                                  string typeName,
                                  bool explicitly)
            : base("xUnit")
        {
            if (typeName == null)
                throw new ArgumentNullException("typeName");

            this.assemblyLocation = assemblyLocation;
            this.typeName = typeName;
            this.explicitly = explicitly;
        }

        public string AssemblyLocation
        {
            get { return assemblyLocation; }
        }

        public ITestClassCommand Command
        {
            get { return command; }
            set { command = value; }
        }

        public bool Explicitly
        {
            get { return explicitly; }
        }

        public string TypeName
        {
            get { return typeName; }
        }

        public bool Equals(XunitTestClassTask xunitTestClassTask)
        {
            if (xunitTestClassTask == null || !base.Equals(xunitTestClassTask))
                return false;

            return (Equals(assemblyLocation, xunitTestClassTask.assemblyLocation) &&
                    Equals(typeName, xunitTestClassTask.typeName) &&
                    explicitly == xunitTestClassTask.explicitly);
        }

        public override bool Equals(object obj)
        {
            return (this == obj || Equals(obj as XunitTestClassTask));
        }

        public override void SaveXml(XmlElement element)
        {
            base.SaveXml(element);
            SetXmlAttribute(element, "TypeName", TypeName);
            SetXmlAttribute(element, "AssemblyLocation", AssemblyLocation);
            SetXmlAttribute(element, "Explicitly", Explicitly ? "true" : "false");
        }
    }
}