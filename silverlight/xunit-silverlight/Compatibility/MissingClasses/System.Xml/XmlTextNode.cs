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

        private static string SimpleXmlEscape(string xml)
        {
            return xml.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
        }

        public override string OuterXml
        {
            get { return SimpleXmlEscape(Value); }
        }
    }
}