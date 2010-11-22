namespace Xunit
{
    internal class XmlTextNode : XmlNode
    {
        internal XmlTextNode(string value, XmlDocument ownerDocument) : base("#text", ownerDocument)
        {
            Value = value;
        }

        public override string InnerText
        {
            get { return Value; }
            set { base.InnerText = value; }
        }

        public override string OuterXml
        {
            get { return Value; }
        }
    }
}