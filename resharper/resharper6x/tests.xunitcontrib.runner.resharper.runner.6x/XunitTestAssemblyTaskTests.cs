using System.Xml;
using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests
{
    public class XunitTestAssemblyTaskTests
    {
        [Fact]
        public void TwoTasksWithSameAssemblyLocationReturnTrueForEquals()
        {
            const string assemblyLocation = "C:\\assembly.dll";

            var task1 = new XunitTestAssemblyTask(assemblyLocation);
            var task2 = new XunitTestAssemblyTask(assemblyLocation);

            Assert.NotSame(task1, task2);
            Assert.Equal(task1, task2);
        }

        [Fact]
        public void TwoTasksWithDifferentAssemblyLocationsReturnsFalseForEquals()
        {
            const string assemblyLocation1 = "C:\\assembly1.dll";
            const string assemblyLocation2 = "C:\\assembly2.dll";

            var task1 = new XunitTestAssemblyTask(assemblyLocation1);
            var task2 = new XunitTestAssemblyTask(assemblyLocation2);

            Assert.NotSame(task1, task2);
            Assert.NotEqual(task1, task2);
        }

        [Fact]
        public void TwoTasksWithDifferentAssemblyLocationsReturnsDifferentHashCodes()
        {
            const string assemblyLocation1 = "C:\\assembly1.dll";
            const string assemblyLocation2 = "C:\\assembly2.dll";

            var task1 = new XunitTestAssemblyTask(assemblyLocation1);
            var task2 = new XunitTestAssemblyTask(assemblyLocation2);

            Assert.NotSame(task1, task2);
            Assert.NotEqual(task1.GetHashCode(), task2.GetHashCode());
        }

        [Fact]
        public void CanSerialiseToAndFromXml()
        {
            const string assemblyLocation = "C:\\assembly.dll";

            var task1 = new XunitTestAssemblyTask(assemblyLocation);

            var xmlDocument = new XmlDocument();
            var xmlElement = xmlDocument.CreateElement("root");
            xmlDocument.AppendChild(xmlElement);
            task1.SaveXml(xmlDocument.DocumentElement);

            var task2 = new XunitTestAssemblyTask(xmlDocument.DocumentElement);

            Assert.NotSame(task1, task2);
            Assert.Equal(task1, task2);
        }

        [Fact]
        public void TwoTaskInstancesWithSameAssemblyLocationsHaveDifferentHashCodes()
        {
            const string assemblyLocation = "C:\\assembly.dll";

            var task1 = new XunitTestAssemblyTask(assemblyLocation);
            var task2 = new XunitTestAssemblyTask(assemblyLocation);

            Assert.NotSame(task1, task2);
            Assert.NotEqual(task1.GetHashCode(), task2.GetHashCode());
        }

        [Fact]
        public void TwoDeserialisedTaskInstanceHaveSameHashCodes()
        {
            const string assemblyLocation = "C:\\assembly.dll";

            var task1 = new XunitTestAssemblyTask(assemblyLocation);

            var xmlDocument = new XmlDocument();
            var xmlElement = xmlDocument.CreateElement("root");
            xmlDocument.AppendChild(xmlElement);
            task1.SaveXml(xmlDocument.DocumentElement);

            var task2 = new XunitTestAssemblyTask(xmlDocument.DocumentElement);

            Assert.NotSame(task1, task2);
            Assert.Equal(task1.GetHashCode(), task2.GetHashCode());
        }
    }
}