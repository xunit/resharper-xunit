using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Xunit
{
    // Silverlight only has System.Xml.Linq, but the version independent runner API uses System.Xml
    // Need to be public since it's used by the API
    // This hasn't been run. At all.
    public class XmlDocument : XmlNode
    {
        private XDocument document;

        public XmlDocument()
            : base(null)
        {
        }

        public override XDocument XDocument
        {
            get { return document; }
        }

        public void LoadXml(string xmlFragment)
        {
            document = new XDocument();
        }

        public XmlAttribute CreateAttribute(string name)
        {
            return new XmlAttribute(this, name);
        }

        public XmlElement CreateElement(string name)
        {
            return new XmlElement(this, name);
        }

        public override IList<XmlNode> ChildNodes
        {
            get { return new[] { this }; }
        }

        public override XmlAttributeCollection Attributes
        {
            get { throw new NotImplementedException(); }
        }

        public override string Name
        {
            get { throw new NotImplementedException(); }
        }

        public override string Value
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override string InnerText
        {
            set { throw new NotImplementedException(); }
            get { throw new NotImplementedException(); }
        }

        public void Load(string filename)
        {
            throw new NotImplementedException();
        }

        public void Save(string filename)
        {
            throw new NotImplementedException();
        }
    }

    public class XmlAttribute : XmlNode
    {
        private readonly XAttribute attribute;

        internal XmlAttribute(XmlDocument document, string name)
            : base(document)
        {
            attribute = new XAttribute(name, string.Empty);
        }

        public override string Value
        {
            set { attribute.Value = value; }
            get { throw new NotImplementedException(); }
        }

        internal XAttribute XAttribute
        {
            get { return attribute; }
        }

        public override string InnerText
        {
            set { throw new NotImplementedException(); }
            get { throw new NotImplementedException(); }
        }

        public override XmlAttributeCollection Attributes
        {
            get { throw new NotImplementedException(); }
        }

        public override string Name
        {
            get { return attribute.Name.LocalName; }
        }
    }

    public class XmlElement : XmlNode
    {
        private readonly XElement element;
        private readonly XmlAttributeCollection attributes;

        internal XmlElement(XmlDocument document, string name)
            : base(document)
        {
            element = new XElement(name);
            attributes = new XmlAttributeCollection(this);
        }

        public override string InnerText
        {
            set { element.Value = value; }
            get { throw new NotImplementedException(); }
        }

        public override XmlAttributeCollection Attributes
        {
            get { return attributes; }
        }

        public override string Name
        {
            get { return element.Name.LocalName; }
        }

        public override string Value
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override void AppendChild(XmlNode child)
        {
            base.AppendChild(child);

            element.Add(((XmlElement)child).XElement);
        }

        internal XElement XElement
        {
            get { return element; }
        }

        internal void AddAttribute(XmlAttribute attribute)
        {
            element.Add(attribute.XAttribute);
        }
    }

    public abstract class XmlNode
    {
        private readonly XmlDocument document;
        private readonly IList<XmlNode> children = new List<XmlNode>();

        internal XmlNode(XmlDocument document)
        {
            this.document = document;
        }

        internal XmlDocument OwnerDocument { get { return document; } }

        public abstract string InnerText { set; get; }

        public string OuterXml
        {
            get { return ToString(); }
        }

        public virtual XDocument XDocument
        {
            get { return document.XDocument; }
        }

        public virtual IList<XmlNode> ChildNodes
        {
            get { return children; }
        }

        public abstract XmlAttributeCollection Attributes { get; }

        public abstract string Name { get; }

        public abstract string Value { get; set; }

        public virtual void AppendChild(XmlNode element)
        {
            children.Add(element);
        }

        public XmlNode SelectSingleNode(string xpath)
        {
            throw new NotImplementedException();
        }

        public IList<XmlNode> SelectNodes(string xpath)
        {
            throw new NotImplementedException();
        }
    }

    public class XmlAttributeCollection
    {
        private readonly XmlElement xmlElement;

        public XmlAttributeCollection(XmlElement xmlElement)
        {
            this.xmlElement = xmlElement;
        }

        public void Append(XmlAttribute attribute)
        {
            xmlElement.AddAttribute(attribute);
        }

        public XmlAttribute this[string name]
        {
            get
            {
                // Should this throw if there is no attribute?
                //var xAttribute = xmlElement.XElement.Attribute(name);
                //return xAttribute != null ? xAttribute.Name.LocalName : string.Empty;
                return null;
            }
        }
    }
}