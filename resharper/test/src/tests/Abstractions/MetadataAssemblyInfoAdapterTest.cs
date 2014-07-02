using System;
using System.Linq;
using System.Runtime.InteropServices;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Utils;
using JetBrains.Util;
using NUnit.Framework;
using Xunit.Abstractions;
using XunitContrib.Runner.ReSharper.UnitTestProvider;

namespace XunitContrib.Runner.ReSharper.Tests.Abstractions
{
    public class MetadataAssemblyInfoAdapterTest
    {
        [Test]
        public void Should_return_assembly_path()
        {
            var assembly = GetAssemblyInfo();

            StringAssert.AreEqualIgnoringCase(GetType().Assembly.Location, assembly.AssemblyPath);
        }

        [Test]
        public void Should_return_name()
        {
            var assembly = GetAssemblyInfo();

            Console.WriteLine(GetType().Assembly.FullName);
            Assert.AreEqual(GetType().Assembly.FullName, assembly.Name);
        }

        [Test]
        public void Should_return_custom_attributes()
        {
            var assembly = GetAssemblyInfo();

            var attributes = assembly.GetCustomAttributes(typeof (GuidAttribute).AssemblyQualifiedName).ToList();
            Assert.AreEqual(1, attributes.Count);
            Assert.AreEqual("28713540-e7ee-4951-925c-b1605b615a8a", attributes[0].GetConstructorArguments().First());
        }

        [Test]
        public void Should_return_specific_type_info()
        {
            var assembly = GetAssemblyInfo();

            var type = assembly.GetType(GetType().FullName);
            Assert.NotNull(type);
            Assert.AreEqual(GetType().FullName, type.Name);
        }

        [Test]
        public void Should_return_public_types()
        {
            var assembly = GetAssemblyInfo();

            var typeNames = assembly.GetTypes(false).Select(t => t.Name).ToList();
            Assert.Contains(GetType().FullName, typeNames);
        }

        [Test]
        public void Should_return_public_nested_type()
        {
            var assembly = GetAssemblyInfo();

            var typeNames = assembly.GetTypes(false).Select(t => t.Name).ToList();
            Assert.Contains(typeof(PublicNestedClass).FullName, typeNames);
        }

        [Test]
        public void Should_return_private_types()
        {
            var assembly = GetAssemblyInfo();

            var typeNames = assembly.GetTypes(true).Select(t => t.Name).ToList();
            Assert.Contains(typeof(PrivateClass).FullName, typeNames);
        }

        [Test]
        public void Should_return_private_nested_types()
        {
            var assembly = GetAssemblyInfo();

            var typeNames = assembly.GetTypes(true).Select(t => t.Name).ToList();
            Assert.Contains(typeof(PrivateNestedClass).FullName, typeNames);
        }

        private IAssemblyInfo GetAssemblyInfo()
        {
            using (var loader = new MetadataLoader())
            {
                var assembly = loader.LoadFrom(FileSystemPath.Parse(GetType().Assembly.Location),
                    JetFunc<AssemblyNameInfo>.True);

                return new MetadataAssemblyInfoAdapter(assembly);
            }

            // Ugh. This requires xunit.execution, which is .net 4.5, but if we change
            // this project to be .net 4.5, the ReSharper tests fail...
            //return Reflector.Wrap(GetType().Assembly);
        }

        public class PublicNestedClass
        {
        }

        private class PrivateNestedClass
        {
        }
    }

    class PrivateClass
    {
    }
}