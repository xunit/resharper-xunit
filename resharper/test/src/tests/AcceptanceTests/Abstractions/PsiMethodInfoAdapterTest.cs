using System;
using System.Linq;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;
using Xunit.Abstractions;
using XunitContrib.Runner.ReSharper.UnitTestProvider;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Abstractions
{
    [TestNetFramework4]
    public class PsiMethodInfoAdapterTest : AbstractionsSourceBaseTest
    {
        protected override string Filename 
        {
            get { return "Methods.cs"; }
        }

        private class TestContext
        {
            private readonly ISymbolScope symbolScope;

            public TestContext(IProject project)
            {
                var psiServices = project.GetSolution().GetComponent<IPsiServices>();
                var module = psiServices.Modules.GetPrimaryPsiModule(project);
                symbolScope = psiServices.Symbols.GetSymbolScope(module, project.GetResolveContext(), false, true);
            }

            public ITypeInfo GetType(string type)
            {
                var typeElement = symbolScope.GetTypeElementByCLRName(type);
                Assert.NotNull(typeElement, "Cannot find type {0}", type);
                return new PsiTypeInfoAdapter2(typeElement);
            }

            public IMethodInfo GetMethod(string type, string method, bool includePrivateMethod = false)
            {
                return GetType(type).GetMethod(method, includePrivateMethod);
            }
        }

        private void DoTest(Action<TestContext> action)
        {
            DoTest(project => action(new TestContext(project)));
        }

        [Test]
        public void Should_indicate_if_method_is_abstract()
        {
            DoTest(context =>
            {
                var methodInfo = context.GetMethod("Foo.ClassWithMethods", "NormalMethod");
                var abstractMethodInfo = context.GetMethod("Foo.AbstractClassWithMethods", "AbstractMethod");

                Assert.False(methodInfo.IsAbstract);
                Assert.True(abstractMethodInfo.IsAbstract);
            });
        }

        [Test]
        public void Should_indicate_if_method_is_a_generic_method()
        {
            DoTest(context =>
            {
                var methodInfo = context.GetMethod("Foo.ClassWithMethods", "NormalMethod");
                var genericMethodInfo = context.GetMethod("Foo.ClassWithMethods", "GenericMethod");

                Assert.False(methodInfo.IsGenericMethodDefinition);
                Assert.True(genericMethodInfo.IsGenericMethodDefinition);
            });
        }

        [Test]
        public void Should_indicate_if_closed_generic_method_is_generic_definition()
        {
            DoTest(context =>
            {
                var methodInfo = context.GetMethod("Foo.ClassWithMethods", "GenericMethod");

                var argsMethod = context.GetMethod("Foo.ClassWithMethods", "Method3");
                var substitutions = argsMethod.GetParameters().Select(p => p.ParameterType).ToArray();

                var closedMethodInfo = methodInfo.MakeGenericMethod(substitutions[0]);

                Assert.True(methodInfo.IsGenericMethodDefinition);
                Assert.False(closedMethodInfo.IsGenericMethodDefinition);
            });
        }

        [Test]
        public void Should_indicate_if_method_is_generic_in_an_open_generic_class()
        {
            DoTest(context =>
            {
                var type = context.GetType("Foo.GenericType`1");
                var methodInfo = type.GetMethod("NormalMethod", false);
                var genericMethodInfo = type.GetMethod("GenericMethod", false);

                Assert.False(methodInfo.IsGenericMethodDefinition);
                Assert.False(genericMethodInfo.IsGenericMethodDefinition);
            });
        }

        [Test]
        public void Should_indicate_if_method_is_generic_in_a_closed_generic_class()
        {
            DoTest(context =>
            {
                var closedGenericTypeInfo = context.GetType("Foo.DerivedClosedGenericType").BaseType;

                var methodInfo = closedGenericTypeInfo.GetMethod("NormalMethod", false);
                var genericMethodInfo = closedGenericTypeInfo.GetMethod("GenericMethod", false);

                Assert.False(methodInfo.IsGenericMethodDefinition);
                Assert.False(genericMethodInfo.IsGenericMethodDefinition);
            });
        }

        [Test]
        public void Should_indicate_if_method_is_public()
        {
            DoTest(context =>
            {
                var methodInfo = context.GetMethod("Foo.ClassWithMethods", "NormalMethod");
                var privateMethodInfo = context.GetMethod("Foo.ClassWithMethods", "PrivateMethod", true);

                Assert.True(methodInfo.IsPublic);
                Assert.False(privateMethodInfo.IsPublic);
            });
        }

        [Test]
        public void Should_indicate_if_method_is_static()
        {
            DoTest(context =>
            {
                var methodInfo = context.GetMethod("Foo.ClassWithMethods", "NormalMethod");
                var staticMethodInfo = context.GetMethod("Foo.ClassWithMethods", "StaticMethod");

                Assert.False(methodInfo.IsStatic);
                Assert.True(staticMethodInfo.IsStatic);
            });
        }

        [Test]
        public void Should_return_methods_name()
        {
            DoTest(context =>
            {
                var methodInfo = context.GetMethod("Foo.ClassWithMethods", "NormalMethod");
                Assert.AreEqual("NormalMethod", methodInfo.Name);
            });
        }

        [Test]
        public void Should_give_methods_return_type()
        {
            DoTest(context =>
            {
                var methodInfo = context.GetMethod("Foo.ClassWithMethods", "ReturnsString");
                Assert.AreEqual("System.String", methodInfo.ReturnType.Name);
            });
        }

        [Test]
        public void Should_handle_void_return_type()
        {
            DoTest(context =>
            {
                var methodInfo = context.GetMethod("Foo.ClassWithMethods", "NormalMethod");
                Assert.AreEqual("System.Void", methodInfo.ReturnType.Name);
            });
        }

        [Test]
        public void Should_give_return_type_for_open_generic()
        {
            DoTest(context =>
            {
                var methodInfo = context.GetMethod("Foo.ClassWithMethods", "GenericMethod");
                Assert.AreEqual("T", methodInfo.ReturnType.Name);
                Assert.True(methodInfo.ReturnType.IsGenericParameter);
            });
        }

        [Test]
        public void Should_give_return_type_for_closed_generic()
        {
            DoTest(context =>
            {
                var typeInfo = context.GetType("Foo.DerivedClosedGenericType").BaseType;
                var methodInfo = typeInfo.GetMethod("GenericMethod", false);

                Assert.AreEqual("System.String", methodInfo.ReturnType.Name);
                Assert.False(methodInfo.ReturnType.IsGenericParameter);
                Assert.False(methodInfo.ReturnType.IsGenericType);
            });
        }

        [Test]
        public void Should_return_attributes()
        {
            DoTest(context =>
            {
                var methodInfo = context.GetMethod("Foo.ClassWithMethods", "MethodWithAttributes");
                // TODO: Should pass in assembly qualified name
                var attributeArgs = methodInfo.GetCustomAttributes("Foo.CustomAttribute")
                    .Select(a => a.GetConstructorArguments().First()).ToList();
                var expectedArgs = new[] { "Foo", "Bar" };
                CollectionAssert.AreEquivalent(expectedArgs, attributeArgs);
            });
        }

        [Test]
        public void Should_return_empty_list_for_non_generic_method()
        {
            DoTest(context =>
            {
                var method = context.GetMethod("Foo.ClassWithMethods", "NormalMethod");

                var args = method.GetGenericArguments().ToList();
                Assert.IsEmpty(args);
            });
        }

        [Test]
        public void Should_return_generic_arguments_for_generic_method()
        {
            DoTest(context =>
            {
                var methodInfo = context.GetMethod("Foo.ClassWithMethods", "GenericMethod2");
                var args = methodInfo.GetGenericArguments().ToList();
                Assert.AreEqual(2, args.Count);
                Assert.AreEqual("T1", args[0].Name);
                Assert.True(args[0].IsGenericParameter);
                Assert.AreEqual("T2", args[1].Name);
                Assert.True(args[1].IsGenericParameter);
            });
        }

        [Test]
        public void Should_return_substituted_arguments_for_closed_generic_method()
        {
            DoTest(context =>
            {
                var methodInfo = context.GetMethod("Foo.ClassWithMethods", "GenericMethod3");

                var argsMethod = context.GetMethod("Foo.ClassWithMethods", "Method3");
                var substitutions = argsMethod.GetParameters().Select(p => p.ParameterType).ToArray();

                var closedMethod = methodInfo.MakeGenericMethod(substitutions);

                var args = closedMethod.GetGenericArguments().ToList();
                Assert.AreEqual(3, args.Count);
                Assert.AreEqual("System.String", args[0].Name);
                Assert.False(args[0].IsGenericParameter);
                Assert.AreEqual("System.Int32", args[1].Name);
                Assert.False(args[1].IsGenericParameter);
                Assert.AreEqual("System.Double", args[2].Name);
                Assert.False(args[2].IsGenericParameter);
            });
        }

        [Test]
        public void Should_return_parameter_information()
        {
            DoTest(context =>
            {
                var methodInfo = context.GetMethod("Foo.ClassWithMethods", "WithParameters");
                var parameters = methodInfo.GetParameters().ToList();
                Assert.AreEqual(2, parameters.Count);
                Assert.AreEqual("i", parameters[0].Name);
                Assert.AreEqual("System.Int32", parameters[0].ParameterType.Name);
                Assert.AreEqual("s", parameters[1].Name);
                Assert.AreEqual("System.String", parameters[1].ParameterType.Name);
            });
        }

        [Test]
        public void Should_return_empty_list_for_method_with_no_parameters()
        {
            DoTest(context =>
            {
                var methodInfo = context.GetMethod("Foo.ClassWithMethods", "NormalMethod");
                var parameters = methodInfo.GetParameters().ToList();
                Assert.IsEmpty(parameters);
            });
        }

        [Test]
        public void Should_return_parameter_information_for_open_class_generic()
        {
            DoTest(context =>
            {
                var methodInfo = context.GetMethod("Foo.GenericType`1", "NormalMethod");

                var parameters = methodInfo.GetParameters().ToList();
                Assert.AreEqual(1, parameters.Count);
                Assert.AreEqual("t", parameters[0].Name);
                Assert.AreEqual("T", parameters[0].ParameterType.Name);
                Assert.True(parameters[0].ParameterType.IsGenericParameter);
                Assert.False(parameters[0].ParameterType.IsGenericType);
            });
        }

        [Test]
        public void Should_return_parameter_information_for_closed_class_generic()
        {
            DoTest(context =>
            {
                var typeInfo = context.GetType("Foo.DerivedClosedGenericType").BaseType;
                var methodInfo = typeInfo.GetMethod("NormalMethod", false);

                var parameters = methodInfo.GetParameters().ToList();
                Assert.AreEqual(1, parameters.Count);
                Assert.AreEqual("t", parameters[0].Name);
                Assert.AreEqual("System.String", parameters[0].ParameterType.Name);
                Assert.False(parameters[0].ParameterType.IsGenericParameter);
                Assert.False(parameters[0].ParameterType.IsGenericType);
            });
        }

        [Test]
        public void Should_return_parameter_information_for_generic_method()
        {
            DoTest(context =>
            {
                var methodInfo = context.GetMethod("Foo.ClassWithMethods", "GenericMethod");

                var parameters = methodInfo.GetParameters().ToList();
                Assert.AreEqual(1, parameters.Count);
                Assert.AreEqual("t", parameters[0].Name);
                Assert.AreEqual("T", parameters[0].ParameterType.Name);
                Assert.True(parameters[0].ParameterType.IsGenericParameter);
                Assert.False(parameters[0].ParameterType.IsGenericType);
            });
        }

        [Test]
        public void Should_convert_open_generic_method_to_closed_definition()
        {
            DoTest(context =>
            {
                var methodInfo = context.GetMethod("Foo.ClassWithMethods", "GenericMethod3");

                var argsMethod = context.GetMethod("Foo.ClassWithMethods", "Method3");
                var substitutions = argsMethod.GetParameters().Select(p => p.ParameterType).ToArray();

                var closedMethod = methodInfo.MakeGenericMethod(substitutions);

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
            });
        }

        [Test]
        public void Should_return_owning_type_info()
        {
            DoTest(context =>
            {
                var methodInfo = context.GetMethod("Foo.ClassWithMethods", "NormalMethod");
                var typeInfo = methodInfo.Type;
                Assert.AreEqual("Foo.ClassWithMethods", typeInfo.Name);
            });
        }
    }
}