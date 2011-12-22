using System.Xml;

namespace JetBrains.ReSharper.UnitTestFramework
{
    public interface IUnitTestElementSerializer
    {
        void SerializeElement(XmlElement parent, IUnitTestElement element);
        IUnitTestElement DeserializeElement(XmlElement parent, IUnitTestElement parentElement);
    }
}