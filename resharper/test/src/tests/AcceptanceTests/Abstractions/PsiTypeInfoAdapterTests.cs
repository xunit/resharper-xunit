using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;
using Xunit.Abstractions;
using XunitContrib.Runner.ReSharper.UnitTestProvider;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Abstractions
{
    [TestNetFramework4]
    public class PsiTypeInfoAdapterTests : AbstractionsSourceBaseTest
    {
        protected override string Filename
        {
            get { return "Types.cs"; }
        }

        private void DoTest(string type, Action<ITypeInfo> action)
        {
            DoTest(project =>
            {
                var psiServices = project.GetSolution().GetComponent<IPsiServices>();
                var module = psiServices.Modules.GetPrimaryPsiModule(project);
                var symbolScope = psiServices.Symbols.GetSymbolScope(module, project.GetResolveContext(), false, true);
                var typeElement = symbolScope.GetTypeElementByCLRName(type);
                Assert.NotNull(typeElement);
                var typeInfo = new PsiTypeInfoAdapter2(typeElement, typeElement.IdSubstitution);
                action(typeInfo);
            });
        }

        private void DoTest(IEnumerable<string> types, Action<IList<ITypeInfo>> action)
        {
            DoTest(project =>
            {
                var psiServices = project.GetSolution().GetComponent<IPsiServices>();
                var module = psiServices.Modules.GetPrimaryPsiModule(project);
                var symbolScope = psiServices.Symbols.GetSymbolScope(module, project.GetResolveContext(), false, true);

                var typeInfos = (from type in types
                    let typeElement = symbolScope.GetTypeElementByCLRName(type)
                    select (ITypeInfo) new PsiTypeInfoAdapter2(typeElement, typeElement.IdSubstitution)).ToList();

                Assert.IsNotEmpty(typeInfos);
                action(typeInfos);
            });
        }

        [Test]
        public void Should_return_containing_assembly()
        {
            DoTest("Foo.BaseType", type => Assert.AreEqual("TestProject", type.Assembly.Name));
        }

        [Test]
        public void Should_return_base_type_of_object()
        {
            DoTest("Foo.BaseType", type => Assert.AreEqual(typeof(object).FullName, type.BaseType.Name));
        }

        [Test]
        public void Should_return_base_type_for_derived_class()
        {
            DoTest("Foo.DerivedType", type => Assert.AreEqual("Foo.BaseType", type.BaseType.Name));
        }

        [Test]
        public void Should_return_list_of_implemented_interfaces()
        {
            DoTest("Foo.TypeWithInterfaces", type =>
            {
                var interfaceNames = type.Interfaces.Select(t => t.Name);
                var expected = new[]
                {
                    typeof (IDisposable).FullName,
                    typeof (IEnumerable<string>).FullName,
                    typeof (IEnumerable).FullName
                };
                CollectionAssert.AreEquivalent(expected, interfaceNames);
            });
        }

        [Test]
        public void Should_return_empty_list_if_no_interfaces()
        {
            DoTest("Foo.DerivedType", type => CollectionAssert.IsEmpty(type.Interfaces));
        }

        [Test]
        public void Should_indicate_if_type_is_abstract()
        {
            DoTest(new[] {"Foo.BaseType", "Foo.DerivedType"}, types =>
            {
                Assert.True(types[0].IsAbstract);
                Assert.False(types[1].IsAbstract);
            });
        }

        [Test]
        public void Should_indicate_if_type_is_closed_generic_type()
        {
            DoTest("Foo.DerivedClosedGenericType", type =>
            {
                var baseType = type.BaseType;
                Assert.IsTrue(baseType.IsGenericType);
                Assert.IsFalse(type.IsGenericType);
            });
        }

        [Test]
        public void Should_indicate_if_type_is_generic_type_declaration()
        {
            DoTest("Foo.GenericType`1", type => Assert.IsTrue(type.IsGenericType));
        }

        [Test]
        public void Should_indicate_if_type_represents_generic_parameter()
        {
            DoTest("Foo.GenericType`1", type =>
            {
                var genericParameterType = type.GetGenericArguments().First();
                Assert.IsTrue(genericParameterType.IsGenericParameter);
                Assert.IsFalse(type.IsGenericParameter);
            });

            DoTest("Foo.DerivedClosedGenericType", type =>
            {
                var baseType = type.BaseType;
                var genericParameterType = baseType.GetGenericArguments().First();
                Assert.IsFalse(genericParameterType.IsGenericParameter);
            });
        }


        [Test]
        public void Should_return_generic_arguments()
        {
            DoTest("Foo.DerivedClosedGenericType2", type =>
            {
                var baseType = type.BaseType;
                var args = baseType.GetGenericArguments().ToList();

                Assert.AreEqual(2, args.Count);
                Assert.AreEqual(typeof(string).FullName, args[0].Name);
                Assert.AreEqual(typeof(int).FullName, args[1].Name);
            });
        }

        [Test]
        public void Should_indicate_if_type_is_sealed()
        {
            DoTest(new[] { "Foo.BaseType", "Foo.SealedType" }, types =>
            {
                Assert.IsFalse(types[0].IsSealed);
                Assert.IsTrue(types[1].IsSealed);
            });
        }

        [Test]
        public void Should_indicate_if_type_is_value_type()
        {
            DoTest(new[] { "Foo.BaseType", "Foo.MyValueType" }, types =>
            {
                Assert.IsFalse(types[0].IsValueType);
                Assert.IsTrue(types[1].IsValueType);
            });
        }

        [Test]
        public void Should_return_type_name()
        {
            const string fullName = "Foo.BaseType";
            DoTest(fullName, type => Assert.AreEqual(fullName, type.Name));
        }

        [Test]
        public void Should_return_type_name_for_open_generic_type()
        {
            const string fullName = "Foo.GenericType`1";
            DoTest(fullName, type => Assert.AreEqual(fullName, type.Name));
        }

        [Test]
        public void Should_return_inherited_attributes()
        {
            DoTest(new[] {"Foo.BaseType", "Foo.DerivedType" }, types =>
            {
                var baseType = types[0];
                var derivedType = types[1];

                // TODO: Should pass in assembly qualified name
                var baseAttributes = baseType.GetCustomAttributes("Foo.CustomAttribute");
                var derivedAttributes = derivedType.GetCustomAttributes("Foo.CustomAttribute");

                Assert.AreEqual("Foo", baseAttributes.First().GetConstructorArguments().First());
                Assert.AreEqual("Foo", derivedAttributes.First().GetConstructorArguments().First());
            });
        }

        [Test]
        public void Should_return_multiple_attribute_instances()
        {
            DoTest("Foo.TypeWithInterfaces", type =>
            {
                // TODO: Should pass in assembly qualified name
                var attributeArgs = type.GetCustomAttributes("Foo.CustomAttribute")
                    .Select(a => a.GetConstructorArguments().First()).ToList();
                var expectedArgs = new[] { "Foo", "Bar" };
                CollectionAssert.AreEquivalent(expectedArgs, attributeArgs);
            });
        }

        [Test]
        public void Should_return_specific_method()
        {
            DoTest("Foo.DerivedType", type =>
            {
                var method = type.GetMethod("PublicMethod", false);

                Assert.NotNull(method);
                Assert.AreEqual("PublicMethod", method.Name);
            });
        }

        [Test]
        public void Should_not_return_private_method()
        {
            DoTest("Foo.DerivedType", type =>
            {
                var method = type.GetMethod("PrivateMethod", false);

                Assert.Null(method);
            });
        }

        [Test]
        public void Should_return_specific_private_method()
        {
            DoTest("Foo.DerivedType", type =>
            {
                var method = type.GetMethod("PrivateMethod", true);

                Assert.NotNull(method);
                Assert.AreEqual("PrivateMethod", method.Name);
            });
        }

        [Test]
        public void Should_return_public_methods()
        {
            DoTest("Foo.DerivedType", type =>
            {
                var methodNames = type.GetMethods(false).Select(m => m.Name).ToList();
                Assert.Contains("PublicMethod", methodNames);
                CollectionAssert.DoesNotContain(methodNames, "PrivateMethod");
            });
        }

        [Test]
        public void Should_return_inherited_methods()
        {
            DoTest("Foo.DerivedType", type =>
            {
                var methodNames = type.GetMethods(false).Select(m => m.Name).ToList();
                Assert.Contains("MethodOnBaseClass", methodNames);
            });
        }

        [Test]
        public void Should_return_private_methods()
        {
            DoTest("Foo.DerivedType", type =>
            {
                var methodNames = type.GetMethods(true).Select(m => m.Name).ToList();
                Assert.Contains("PrivateMethod", methodNames);
            });
        }

        [Test]
        public void Should_return_owning_assembly()
        {
            // TODO: Should be assembly qualified name?
            // i.e. TestProject, Version=1.0.0.0, Culture=null, PublicKeyToken=null
            DoTest("Foo.BaseType", type => Assert.AreEqual(type.Assembly.Name, "TestProject"));
        }
    }
}