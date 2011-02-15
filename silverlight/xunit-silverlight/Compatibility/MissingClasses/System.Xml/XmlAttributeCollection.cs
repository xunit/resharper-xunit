using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System.Xml
{
    public class XmlAttributeCollection : IEnumerable<XmlAttribute>
    {
        private readonly IList<XmlAttribute> attributes = new List<XmlAttribute>();

        public void Append(XmlAttribute attr)
        {
            attributes.Add(attr);
        }

        public XmlAttribute this[string name]
        {
            get { return attributes.SingleOrDefault(x => x.Name == name);}
        }

        public IEnumerator<XmlAttribute> GetEnumerator()
        {
            return attributes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}