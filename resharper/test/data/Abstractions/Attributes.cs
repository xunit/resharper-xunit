namespace Foo
{
    public class CtorArgsAttribute : Attribute
    {
        public CtorArgsAttribute(string s, int i, Type t)
        {
        }
    }

    [CtorArgs("hello", 42, typeof(string))]
    public class HasCtorArgs
    {
    }

    public class NamedArgsAttribute : Attribute
    {
        public string StringProperty { get; set; }
        public int IntProperty { get; set; }

        public string StringField;
        public int IntField;

        public string UnsetStringProperty { get; set; }
        public int UnsetIntProperty { get; set; }
    }

    [NamedArgs(StringProperty = "hello", StringField = "world", IntProperty = 42, IntField = 24)]
    public class HasNamedArgs
    {
    }

    public class BaseAttribute : Attribute
    {
    }

    public class DerivedAttribute : BaseAttribute
    {
    }

    [Base]
    [Derived]
    public class AttributedAttribute : Attribute
    {
    }

    [Attributed]
    public class HasAttributed
    {
    }
}
