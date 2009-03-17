using System;
using System.Xml;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper
{
    [Serializable]
    public class XunitTestMethodTask : RemoteTask, IEquatable<XunitTestMethodTask>
    {
        readonly bool explicitly;
        readonly string testMethod;
        readonly string testType;

        public XunitTestMethodTask(XmlElement element)
            : base(element)
        {
            testMethod = GetXmlAttribute(element, "TestMethod");
            testType = GetXmlAttribute(element, "TestType");
            explicitly = GetXmlAttribute(element, "Explicitly") == "true";
        }

        public XunitTestMethodTask(string testType,
                                   string testMethod,
                                   bool explicitly)
            : base("xUnit")
        {
            if (testMethod == null)
                throw new ArgumentNullException("testMethod");

            if (testType == null)
                throw new ArgumentNullException("testType");

            this.testType = testType;
            this.testMethod = testMethod;
            this.explicitly = explicitly;
        }

        public bool Explicitly
        {
            get { return explicitly; }
        }

        public string TestMethod
        {
            get { return testMethod; }
        }

        public string TestType
        {
            get { return testType; }
        }

        public bool Equals(XunitTestMethodTask xunitTestMethodTask)
        {
            if (xunitTestMethodTask == null || !base.Equals(xunitTestMethodTask))
                return false;

            return Equals(testType, xunitTestMethodTask.testType) &&
                   Equals(testMethod, xunitTestMethodTask.testMethod) &&
                   explicitly == xunitTestMethodTask.explicitly;
        }

        public override bool Equals(object obj)
        {
            return (this == obj || Equals(obj as XunitTestMethodTask));
        }

        public override void SaveXml(XmlElement element)
        {
            base.SaveXml(element);
            SetXmlAttribute(element, "TestMethod", TestMethod);
            SetXmlAttribute(element, "TestType", TestType);
            SetXmlAttribute(element, "Explicitly", Explicitly ? "true" : "false");
        }
    }
}