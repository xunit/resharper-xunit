namespace Xunit
{
    public class XmlAttribute
    {
        internal XmlAttribute(string name)
        {
            Name = name;
            Value = string.Empty;
        }

        public string Name { get; set; }
        public string Value { get; set; }
    }
}