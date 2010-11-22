using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Xunit
{
    public class XmlDocument : XmlNode
    {
        public XmlDocument() : base(null, null)
        {
        }

        public XmlAttribute CreateAttribute(string name)
        {
            return new XmlAttribute(name);
        }

        public XmlNode CreateElement(string name)
        {
            return new XmlElement(name, this);
        }

        public void LoadXml(string xmlFragment)
        {
            ClearChildren();

            using (var textReader = new StringReader(xmlFragment))
            using(var xmlReader = XmlReader.Create(textReader))
            {
                var xDocument = XDocument.Load(xmlReader);

                // Walk the new document, creating nodes...
                Blah(xDocument, this);
            }
        }

        private void Blah(XContainer parentXElement, XmlNode parentXmlNode)
        {
            foreach (var xNode in parentXElement.Nodes())
            {
                switch (xNode.NodeType)
                {
                    case XmlNodeType.Element:
                        {
                            var xElement = (XElement) xNode;
                            var element = CreateElement(xElement.Name.LocalName);
                            parentXmlNode.AppendChild(element);

                            foreach (var xAttribute in xElement.Attributes())
                            {
                                var xmlAttribute = CreateAttribute(xAttribute.Name.LocalName);
                                xmlAttribute.Value = xAttribute.Value;
                                element.Attributes.Append(xmlAttribute);
                            }
                            Blah(xElement, element);
                        }
                        break;
                    case XmlNodeType.Text:
                        {
                            var xText = (XText) xNode;
                            var value = xText.Value.Replace("\n", "\r\n");
                            var text = new XmlTextNode(value, parentXmlNode.OwnerDocument);
                            parentXmlNode.AppendChild(text);
                        }
                        break;
                }
            }
        }
    }
}