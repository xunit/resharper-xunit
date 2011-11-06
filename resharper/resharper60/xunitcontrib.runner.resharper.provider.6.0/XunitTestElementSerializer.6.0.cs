using System.Xml;
using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public interface IUnitTestElementSerializer
    {
        void SerializeElement(XmlElement parent, IUnitTestElement element);
        IUnitTestElement DeserializeElement(XmlElement parent, IUnitTestElement parentElement);
    }
}