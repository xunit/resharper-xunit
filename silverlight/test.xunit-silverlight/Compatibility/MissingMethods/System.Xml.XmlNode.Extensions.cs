using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Xunit
{
    internal static partial class Extensions
    {
        public static XmlNode SelectSingleNode(this XmlNode xmlNode, string xpath)
        {
            using(var xmlReader = new StringReader(xmlNode.OuterXml))
            {
                var xElement = XElement.Load(xmlReader);
                var xPathSelectedElement = xElement.XPathSelectElement(xpath);
                if (xPathSelectedElement == null)
                    return null;

                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xPathSelectedElement.ToString());
                return xmlDocument.ChildNodes[0];
            }
        }

        public static XmlNodeList SelectNodes(this XmlNode xmlNode, string xpath)
        {
            using (var xmlReader = new StringReader(xmlNode.OuterXml))
            {
                var xElement = XElement.Load(xmlReader);
                var xPathSelectedElements = xElement.XPathSelectElements(xpath);
                if (xPathSelectedElements == null)
                    return null;

                var xmlNodes = new XmlNodeList();
                foreach (var xPathSelectedElement in xPathSelectedElements)
                {
                    var xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(xPathSelectedElement.ToString());
                    xmlNodes.Add(xmlDocument.ChildNodes[0]);
                }
                return xmlNodes;
            }
        }
    }
}