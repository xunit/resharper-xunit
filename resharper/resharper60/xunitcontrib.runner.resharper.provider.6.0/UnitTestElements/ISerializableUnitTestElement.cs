using System.Xml;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    internal interface ISerializableUnitTestElement
    {
        void WriteToXml(XmlElement element);
    }
}