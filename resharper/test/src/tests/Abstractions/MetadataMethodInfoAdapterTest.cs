using System;
using System.Linq;
using JetBrains.DataFlow;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.Metadata.Utils;
using JetBrains.Util;
using NUnit.Core.Extensibility;
using NUnit.Framework;
using Xunit.Abstractions;
using XunitContrib.Runner.ReSharper.UnitTestProvider;

namespace XunitContrib.Runner.ReSharper.Tests.Abstractions
{
    public class MetadataMethodInfoAdapterTest
    {
        [Test]
        public void Should_indicate_if_method_is_abstract()
        {
            var methodInfo = GetMethodInfo(typeof (ClassWithMethods), "NormalMethod");
            var abstractMethodInfo = GetMethodInfo(typeof (AbstractClassWithMethods), "AbstractMethod");

            Assert.False(methodInfo.IsAbstract);
            Assert.True(abstractMethodInfo.IsAbstract);
        }

        [Test]
        public void Should_indicate_if_method_is_a_generic_method()
        {
            var methodInfo = GetMethodInfo(typeof(ClassWithMethods), "NormalMethod");
            var genericMethodInfo = GetMethodInfo(typeof(ClassWithMethods), "GenericMethod");

            Assert.False(methodInfo.IsGenericMethodDefinition);
            Assert.True(genericMethodInfo.IsGenericMethodDefinition);
        }

        [Test]
        public void Should_indicate_if_method_is_generic_in_an_open_generic_class()
        {
            var methodInfo = GetMethodInfo(typeof(GenericType<>), "NormalMethod");
            var genericMethodInfo = GetMethodInfo(typeof(GenericType<>), "GenericMethod");

            Assert.False(methodInfo.IsGenericMethodDefinition);
            Assert.False(genericMethodInfo.IsGenericMethodDefinition);
        }

        [Test]
        public void Should_indicate_if_method_is_generic_in_an_closed_generic_class()
        {
            var methodInfo = GetMethodInfo(typeof(GenericType<string>), "NormalMethod");
            var genericMethodInfo = GetMethodInfo(typeof(GenericType<string>), "GenericMethod");

            Assert.False(methodInfo.IsGenericMethodDefinition);
            Assert.False(genericMethodInfo.IsGenericMethodDefinition);
        }

        [Test]
        public void Should_indicate_if_method_is_public()
        {
            var methodInfo = GetMethodInfo(typeof(ClassWithMethods), "NormalMethod");
            var privateMethodInfo = GetMethodInfo(typeof(ClassWithMethods), "PrivateMethod", true);

            Assert.True(methodInfo.IsPublic);
            Assert.False(privateMethodInfo.IsPublic);
        }

        [Test]
        public void Should_indicate_if_method_is_static()
        {
            var methodInfo = GetMethodInfo(typeof(ClassWithMethods), "NormalMethod");
            var privateMethodInfo = GetMethodInfo(typeof(ClassWithMethods), "StaticMethod");

            Assert.False(methodInfo.IsStatic);
            Assert.True(privateMethodInfo.IsStatic);
        }

        [Test]
        public void Should_return_methods_name()
        {
            var methodInfo = GetMethodInfo(typeof(ClassWithMethods), "NormalMethod");
            Assert.AreEqual("NormalMethod", methodInfo.Name);
        }

        [Test]
        public void Should_give_methods_return_type()
        {
            var methodInfo = GetMethodInfo(typeof (ClassWithMethods), "ReturnsString");

            Assert.AreEqual("System.String", methodInfo.ReturnType.Name);
        }

        [Test]
        public void Should_handle_void_return_type()
        {
            var methodInfo = GetMethodInfo(typeof (ClassWithMethods), "NormalMethod");

            Assert.AreEqual("System.Void", methodInfo.ReturnType.Name);
        }

        [Test]
        public void Should_give_return_type_for_open_generic()
        {
            var methodInfo = GetMethodInfo(typeof (ClassWithMethods), "GenericMethod");

            Assert.AreEqual("T", methodInfo.ReturnType.Name);
            Assert.True(methodInfo.ReturnType.IsGenericParameter);
        }

        [Test]
        public void Should_give_return_type_for_closed_generic()
        {
            var methodInfo = GetMethodInfo(typeof(GenericType<string>), "GenericMethod");

            Assert.AreEqual("System.String", methodInfo.ReturnType.Name);
            Assert.False(methodInfo.ReturnType.IsGenericParameter);
            Assert.False(methodInfo.ReturnType.IsGenericType);
        }

        [Test]
        public void Should_return_attributes()
        {
            var method = GetMethodInfo(typeof(ClassWithMethods), "MethodWithAttributes");

            var attributeType = typeof(CustomAttribute);
            var attributeArgs = method.GetCustomAttributes(attributeType.AssemblyQualifiedName)
                .Select(a => a.GetConstructorArguments().First()).ToList();
            var expectedArgs = new[] { "Foo", "Bar" };
            CollectionAssert.AreEquivalent(expectedArgs, attributeArgs);
        }

        [Test]
        public void Should_return_generic_arguments_for_generic_method()
        {
            var method = GetMethodInfo(typeof (ClassWithMethods), "GenericMethod2");

            var args = method.GetGenericArguments().ToList();
            Assert.AreEqual(2, args.Count);
            Assert.AreEqual("T1", args[0].Name);
            Assert.True(args[0].IsGenericParameter);
            Assert.AreEqual("T2", args[1].Name);
            Assert.True(args[1].IsGenericParameter);
        }

        [Test]
        public void Should_return_parameter_information()
        {
            var method = GetMethodInfo(typeof(ClassWithMethods), "WithParameters");

            var parameters = method.GetParameters().ToList();
            Assert.AreEqual(2, parameters.Count);
            Assert.AreEqual("i", parameters[0].Name);
            Assert.AreEqual("System.Int32", parameters[0].ParameterType.Name);
            Assert.AreEqual("s", parameters[1].Name);
            Assert.AreEqual("System.String", parameters[1].ParameterType.Name);
        }

        [Test]
        public void Should_return_empty_list_for_method_with_no_parameters()
        {
            var method = GetMethodInfo(typeof (ClassWithMethods), "NormalMethod");

            var parameters = method.GetParameters().ToList();
            Assert.IsEmpty(parameters);
        }

        [Test]
        public void Should_return_parameter_information_for_open_class_generic()
        {
            var method = GetMethodInfo(typeof (GenericType<>), "NormalMethod");

            var parameters = method.GetParameters().ToList();
            Assert.AreEqual(1, parameters.Count);
            Assert.AreEqual("t", parameters[0].Name);
            Assert.AreEqual("T", parameters[0].ParameterType.Name);
            Assert.True(parameters[0].ParameterType.IsGenericParameter);
            Assert.False(parameters[0].ParameterType.IsGenericType);
        }

        [Test]
        public void Should_return_parameter_information_for_closed_class_generic()
        {
            var method = GetMethodInfo(typeof(GenericType<string>), "NormalMethod");

            var parameters = method.GetParameters().ToList();
            Assert.AreEqual(1, parameters.Count);
            Assert.AreEqual("t", parameters[0].Name);
            Assert.AreEqual("System.String", parameters[0].ParameterType.Name);
            Assert.False(parameters[0].ParameterType.IsGenericParameter);
            Assert.False(parameters[0].ParameterType.IsGenericType);
        }

        [Test]
        public void Should_return_parameter_information_for_generic_method()
        {
            var method = GetMethodInfo(typeof (ClassWithMethods), "GenericMethod");

            var parameters = method.GetParameters().ToList();
            Assert.AreEqual(1, parameters.Count);
            Assert.AreEqual("t", parameters[0].Name);
            Assert.AreEqual("T", parameters[0].ParameterType.Name);
            Assert.True(parameters[0].ParameterType.IsGenericParameter);
            Assert.False(parameters[0].ParameterType.IsGenericType);
        }

        [Test]
        public void Should_convert_open_generic_method_to_closed_definition()
        {
            var method = GetMethodInfo(typeof (ClassWithMethods), "GenericMethod3");

            var closedMethod = method.MakeGenericMethod(GetTypeInfo(typeof (string)), GetTypeInfo(typeof (int)),
                GetTypeInfo(typeof (double)));

            Assert.AreEqual("System.Double", closedMethod.ReturnType.Name);
            Assert.False(closedMethod.ReturnType.IsGenericParameter);
            Assert.False(closedMethod.ReturnType.IsGenericType);

            var parameters = closedMethod.GetParameters().ToList();
            Assert.AreEqual(2, parameters.Count);
            Assert.AreEqual("t1", parameters[0].Name);
            Assert.AreEqual("System.String", parameters[0].ParameterType.Name);
            Assert.False(parameters[0].ParameterType.IsGenericParameter);
            Assert.AreEqual("t2", parameters[1].Name);
            Assert.AreEqual("System.Int32", parameters[1].ParameterType.Name);
            Assert.False(parameters[1].ParameterType.IsGenericParameter);
        }

        [Test]
        public void Should_return_owning_type_info()
        {
            var method = GetMethodInfo(typeof (ClassWithMethods), "NormalMethod");

            var typeInfo = method.Type;

            Assert.AreEqual(typeof(ClassWithMethods).FullName, typeInfo.Name);
        }

        private static ITypeInfo GetTypeInfo(Type type)
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


        private static IMethodInfo GetMethodInfo(Type type, string methodName, bool includePrivateMethod = false)
        {
            var typeInfo = GetTypeInfo(type);
            var methodInfo = typeInfo.GetMethod(methodName, includePrivateMethod);
            Assert.NotNull(methodInfo, "Cannot find method {0}.{1}", type.FullName, methodName);
            return methodInfo;
        }
    }

    public class ClassWithMethods
    {
        public void NormalMethod()
        {
        }

        private void PrivateMethod()
        {
        }

        public static void StaticMethod()
        {
        }

        [Custom("Foo"), Custom("Bar")]
        public void MethodWithAttributes()
        {
        }

        public T GenericMethod<T>(T t)
        {
            return default(T);
        }

        public void GenericMethod2<T1, T2>(T1 t1, T2 t2)
        {
        }

        public TResult GenericMethod3<T1, T2, TResult>(T1 t1, T2 t2)
        {
            throw new NotImplementedException();
        }

        public void WithParameters(int i, string s)
        {
        }

        public string ReturnsString()
        {
            return "foo";
        }
    }

    public abstract class AbstractClassWithMethods
    {
        public abstract void AbstractMethod();
    }
}