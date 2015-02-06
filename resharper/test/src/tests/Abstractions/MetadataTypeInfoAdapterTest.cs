using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.DataFlow;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.Metadata.Utils;
using JetBrains.Util;
using NUnit.Framework;
using Xunit.Abstractions;
using XunitContrib.Runner.ReSharper.UnitTestProvider;

namespace XunitContrib.Runner.ReSharper.Tests.Abstractions
{
    public class MetadataTypeInfoAdapterTest
    {
        [Test]
        public void Should_return_containing_assembly()
        {
            var type = GetTypeInfo(GetType());

            Assert.AreEqual(GetType().Assembly.FullName, type.Assembly.Name);
        }

        [Test]
        public void Should_return_base_type_of_object()
        {
            var type = GetTypeInfo(typeof(BaseType));

            Assert.AreEqual(typeof (object).FullName, type.BaseType.Name);
        }

        [Test]
        public void Should_return_base_type_for_derived_class()
        {
            var typeInfo = GetTypeInfo(typeof (DerivedType));

            Assert.AreEqual(typeof (BaseType).FullName, typeInfo.BaseType.Name);
        }

        [Test]
        public void Should_return_list_of_implemented_interfaces()
        {
            var type = GetTypeInfo(typeof (TypeWithInterfaces));

            var interfaceNames = type.Interfaces.Select(t => t.Name);
            var expected = new[]
            {
                typeof (IDisposable).FullName,
                typeof (IEnumerable<string>).FullName,
                typeof (IEnumerable).FullName
            };
            CollectionAssert.AreEquivalent(expected, interfaceNames);
        }

        [Test]
        public void Should_return_empty_list_if_no_interfaces()
        {
            var type = GetTypeInfo(typeof (DerivedType));

            CollectionAssert.IsEmpty(type.Interfaces);
        }

        [Test]
        public void Should_indicate_if_type_is_abstract()
        {
            var type = GetTypeInfo(typeof (BaseType));
            var derivedType = GetTypeInfo(typeof (DerivedType));

            Assert.True(type.IsAbstract);
            Assert.False(derivedType.IsAbstract);
        }

        [Test]
        public void Should_indicate_if_type_is_generic_type()
        {
            var type = GetTypeInfo(typeof(TypeWithInterfaces));
            var genericInstance = new GenericType<string>();
            var genericType = GetTypeInfo(genericInstance.GetType());

            Assert.IsFalse(type.IsGenericType);
            Assert.IsTrue(genericType.IsGenericType);
        }

        [Test]
        public void Should_indicate_if_type_represents_generic_parameter()
        {
            var type = GetTypeInfo(typeof (TypeWithInterfaces));
            var genericParameterType = GetTypeInfo(typeof (IEnumerable<string>)).GetGenericArguments().First();
            var openGenericParameterType = GetTypeInfo(typeof (IEnumerable<>)).GetGenericArguments().First();

            Assert.IsFalse(type.IsGenericParameter);
            Assert.IsFalse(genericParameterType.IsGenericParameter);
            Assert.IsTrue(openGenericParameterType.IsGenericParameter);
        }


        [Test]
        public void Should_return_generic_arguments()
        {
            var type = GetTypeInfo(typeof (IDictionary<string, int>));
            var args = type.GetGenericArguments().ToList();

            Assert.AreEqual(2, args.Count);
            Assert.AreEqual(typeof(string).FullName, args[0].Name);
            Assert.AreEqual(typeof(int).FullName, args[1].Name);
        }

        [Test]
        public void Should_indicate_if_type_is_sealed()
        {
            var type = GetTypeInfo(typeof (BaseType));
            var sealedType = GetTypeInfo(typeof (SealedType));

            Assert.IsFalse(type.IsSealed);
            Assert.IsTrue(sealedType.IsSealed);
        }

        [Test]
        public void Should_indicate_if_type_is_value_type()
        {
            var type = GetTypeInfo(typeof (BaseType));
            var valueType = GetTypeInfo(typeof (MyValueType));

            Assert.IsFalse(type.IsValueType);
            Assert.IsTrue(valueType.IsValueType);
        }

        [Test]
        public void Should_return_type_name()
        {
            var type = GetTypeInfo(typeof (BaseType));

            Assert.AreEqual(typeof(BaseType).FullName, type.Name);
        }

        [Test]
        public void Should_return_type_name_for_generic_type()
        {
            var type = typeof(GenericType<>);
            var typeInfo = GetTypeInfo(type);

            Assert.AreEqual(type.FullName, typeInfo.Name);
        }

        [Test]
        public void Should_return_inherited_attributes()
        {
            var baseType = GetTypeInfo(typeof (BaseType));
            var derivedType = GetTypeInfo(typeof (DerivedType));

            var attributeType = typeof (CustomAttribute);
            var attributeName = attributeType.AssemblyQualifiedName;

            var baseAttributes = baseType.GetCustomAttributes(attributeName);
            var derivedAttributes = derivedType.GetCustomAttributes(attributeName);

            Assert.AreEqual("Foo", baseAttributes.First().GetConstructorArguments().First());
            Assert.AreEqual("Foo", derivedAttributes.First().GetConstructorArguments().First());
        }

        [Test]
        public void Should_return_multiple_attribute_instances()
        {
            var type = GetTypeInfo(typeof (TypeWithInterfaces));

            var attributeType = typeof (CustomAttribute);
            var attributeArgs = type.GetCustomAttributes(attributeType.AssemblyQualifiedName)
                .Select(a => a.GetConstructorArguments().First()).ToList();
            var expectedArgs = new[] { "Foo", "Bar" };
            CollectionAssert.AreEquivalent(expectedArgs, attributeArgs);
        }

        [Test]
        public void Should_return_specific_method()
        {
            var type = GetTypeInfo(typeof (DerivedType));

            var method = type.GetMethod("PublicMethod", false);

            Assert.NotNull(method);
            Assert.AreEqual("PublicMethod", method.Name);
        }

        [Test]
        public void Should_not_return_private_method()
        {
            var type = GetTypeInfo(typeof(DerivedType));

            var method = type.GetMethod("PrivateMethod", false);

            Assert.Null(method);
        }

        [Test]
        public void Should_return_specific_private_method()
        {
            var type = GetTypeInfo(typeof (DerivedType));

            var method = type.GetMethod("PrivateMethod", true);

            Assert.NotNull(method);
            Assert.AreEqual("PrivateMethod", method.Name);
        }

        [Test]
        public void Should_return_public_methods()
        {
            var type = GetTypeInfo(typeof(DerivedType));

            var methodNames = type.GetMethods(false).Select(m => m.Name).ToList();
            Assert.Contains("PublicMethod", methodNames);
        }

        [Test]
        public void Should_return_inherited_methods()
        {
            var type = GetTypeInfo(typeof (DerivedType));

            var methodNames = type.GetMethods(false).Select(m => m.Name).ToList();
            Assert.Contains("MethodOnBaseClass", methodNames);
        }

        [Test]
        public void Should_return_private_methods()
        {
            var type = GetTypeInfo(typeof(DerivedType));

            var methodNames = type.GetMethods(true).Select(m => m.Name).ToList();
            Assert.Contains("PrivateMethod", methodNames);
        }

        [Test]
        public void Should_return_owning_assembly()
        {
            var type = GetTypeInfo(typeof (BaseType));

            Assert.AreEqual(type.Assembly.Name, typeof(BaseType).Assembly.FullName);
        }

        private ITypeInfo GetTypeInfo(Type type)
        {
            return Lifetimes.Using(lifetime =>
            {
                var resolver = new CombiningAssemblyResolver(GacAssemblyResolverFactory.CreateOnCurrentRuntimeGac(),
                    new LoadedAssembliesResolver(lifetime, true));
                using (var loader = new MetadataLoader(resolver))
                {
                    var assembly = loader.LoadFrom(FileSystemPath.Parse(type.Assembly.Location),
                        JetFunc<AssemblyNameInfo>.True);

                    var typeInfo = new MetadataAssemblyInfoAdapter(assembly).GetType(type.FullName);
                    Assert.NotNull(typeInfo, "Cannot load type {0}", type.FullName);
                    return typeInfo;
                }
            });

            // Ugh. This requires xunit.execution, which is .net 4.5, but if we change
            // this project to be .net 4.5, the ReSharper tests fail...
            //var assembly = Xunit.Sdk.Reflector.Wrap(type.Assembly);
            //var typeInfo = assembly.GetType(type.FullName);
            //Assert.NotNull(typeInfo, "Cannot load type {0}", type.FullName);
            //return typeInfo;
        }
    }

    public struct MyValueType
    {
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class CustomAttribute : Attribute
    {
        private readonly string value;

        public CustomAttribute(string value)
        {
            this.value = value;
        }
    }

    [Custom("Foo")]
    public abstract class BaseType
    {
        public void MethodOnBaseClass()
        {
        }
    }

    public class DerivedType : BaseType
    {
        public void PublicMethod()
        {
        }

        private void PrivateMethod()
        {
        }
    }

    [Custom("Foo"), Custom("Bar")]
    public class TypeWithInterfaces : IDisposable, IEnumerable<string>
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IEnumerator<string> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class GenericType<T>
    {
        public void NormalMethod(T t)
        {
        }

        public T GenericMethod()
        {
            return default(T);
        }
    }

    public sealed class SealedType
    {
    }
}