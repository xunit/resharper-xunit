using System.Collections.Generic;
using System.Text;

namespace System.Xml
{
    public abstract class XmlNode
    {
        private readonly string name;
        private readonly XmlDocument ownerDocument;
        private readonly XmlAttributeCollection attributes = new XmlAttributeCollection();
        private readonly IList<XmlNode> children = new List<XmlNode>();

        protected XmlNode(string name, XmlDocument ownerDocument)
        {
            this.name = name;
            this.ownerDocument = ownerDocument;
        }

        internal XmlDocument OwnerDocument { get { return ownerDocument; } }
        public XmlAttributeCollection Attributes { get { return attributes; } }

        public virtual string InnerText
        {
            get
            {
                var stringBuilder = new StringBuilder();
                foreach (var childNode in children)
                {
                    stringBuilder.Append(childNode.InnerText);
                }
                return stringBuilder.ToString();
            }
            set
            {
                // Strictly speaking, we should remove any existing text nodes
                AppendChild(new XmlTextNode(value, ownerDocument));
            }
        }

        protected virtual string Value { get; set; }

        public virtual string OuterXml
        {
            get
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendFormat("<{0}", name);
                foreach (var attribute in attributes)
                {
                    stringBuilder.AppendFormat(" {0}=\"{1}\"", attribute.Name, attribute.Value);
                }
                stringBuilder.Append(">");

                foreach (var childNode in children)
                {
                    stringBuilder.Append(childNode.OuterXml);
                }

                stringBuilder.AppendFormat("</{0}>", name);
                return stringBuilder.ToString();
            }
        }

        public IList<XmlNode> ChildNodes
        {
            get { return children; }
        }

        public string Name
        {
            get { return name; }
        }

        protected void ClearChildren()
        {
            children.Clear();
        }

        internal void AppendChild(XmlNode xmlNode)
        {
            children.Add(xmlNode);
        }
    }
}