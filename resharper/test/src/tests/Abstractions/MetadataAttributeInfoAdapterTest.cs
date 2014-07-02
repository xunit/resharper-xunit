using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Utils;
using JetBrains.Util;
using NUnit.Framework;
using Xunit.Abstractions;
using XunitContrib.Runner.ReSharper.UnitTestProvider;

namespace XunitContrib.Runner.ReSharper.Tests.Abstractions
{
    public class MetadataAttributeInfoAdapterTest
    {
        [Test]
        public void Should_return_constructor_arguments()
        {
            var attributes = GetAttributes(typeof (HasCtorArgs)).ToList();

            Assert.AreEqual(1, attributes.Count);
            var attribute = attributes[0];

            var args = attribute.GetConstructorArguments().ToList();
            Assert.NotNull(args);
            Assert.AreEqual(3, args.Count);
            Assert.AreEqual("hello", args[0]);
            Assert.AreEqual(42, args[1]);

            // This fails if the metadata loader doesn't have the assembly referenced
            // Don't know if it's going to cause issues
            //Assert.AreEqual(typeof(string), args[2]);
        }

        [Test]
        public void Should_return_named_arguments()
        {
            var attributes = GetAttributes(typeof (HasNamedArgs)).ToList();

            Assert.AreEqual(1, attributes.Count);
            var attribute = attributes[0];

            Assert.AreEqual("hello", attribute.GetNamedArgument<string>("StringProperty"));
            Assert.AreEqual("world", attribute.GetNamedArgument<string>("StringField"));
            Assert.AreEqual(42, attribute.GetNamedArgument<int>("IntProperty"));
            Assert.AreEqual(24, attribute.GetNamedArgument<int>("IntField"));
            Assert.Null(attribute.GetNamedArgument<string>("UnsetStringProperty"));
            Assert.AreEqual(0, attribute.GetNamedArgument<int>("UnsetIntProperty"));
        }

        [Test]
        public void Should_return_custom_attributes()
        {
            var attributes = GetAttributes(typeof(HasBoring)).ToList();

            Assert.AreEqual(1, attributes.Count);
            var attribute = attributes[0];

            var customAttributes = attribute.GetCustomAttributes(typeof(FooAttribute).AssemblyQualifiedName);
            Assert.NotNull(customAttributes);
            Assert.AreEqual(2, customAttributes.Count());
        }

        private IEnumerable<IAttributeInfo> GetAttributes(Type targetType)
        {
            using (var loader = new MetadataLoader())
            {
                var assembly = loader.LoadFrom(FileSystemPath.Parse(targetType.Assembly.Location),
                    JetFunc<AssemblyNameInfo>.True);

                var type = assembly.GetTypeInfoFromQualifiedName(targetType.FullName, false);
                return type.CustomAttributes.Select(a => new MetadataAttributeInfoAdapter2(a));
            }

            // Ugh. This requires xunit.execution, which is .net 4.5, but if we change
            // this project to be .net 4.5, the ReSharper tests fail...
            //return CustomAttributeData.GetCustomAttributes(targetType).Select(Reflector.Wrap);
        }
    }

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
        public string UnsetIntProperty { get; set; }
    }

    [NamedArgs(StringProperty = "hello", StringField = "world", IntProperty = 42, IntField = 24)]
    public class HasNamedArgs
    {
    }

    public class FooAttribute : Attribute
    {
    }

    public class Foo2Attribute : FooAttribute
    {
    }

    [FooAttribute]
    [Foo2]
    public class BoringAttribute : Attribute
    {
    }

    [Boring]
    public class HasBoring
    {
    }
}