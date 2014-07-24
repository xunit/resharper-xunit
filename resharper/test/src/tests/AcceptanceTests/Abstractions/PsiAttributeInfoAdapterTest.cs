using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using NUnit.Framework;
using Xunit.Abstractions;
using XunitContrib.Runner.ReSharper.Tests.Abstractions;
using XunitContrib.Runner.ReSharper.UnitTestProvider;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Abstractions
{
    public class PsiAttributeInfoAdapterTest : AbstractionsSourceBaseTest
    {
        protected override string Filename
        {
            get { return "Attributes.cs"; }
        }

        private void DoTest(string type, Action<IList<IAttributeInfo>> action)
        {
            DoTest(project =>
            {
                var psiServices = project.GetSolution().GetComponent<IPsiServices>();
                var module = psiServices.Modules.GetPrimaryPsiModule(project);
                var symbolScope = psiServices.Symbols.GetSymbolScope(module, project.GetResolveContext(), false, true);
                var typeElement = symbolScope.GetTypeElementByCLRName(type);
                Assert.NotNull(typeElement);
                var attributes = from attribute in typeElement.GetAttributeInstances(true)
                    select (IAttributeInfo) new PsiAttributeInfoAdapter2(attribute);
                action(attributes.ToList());
            });
        }

        [Test]
        public void Should_return_constructor_arguments()
        {
            DoTest("Foo.HasCtorArgs", attributes =>
            {
                Assert.AreEqual(1, attributes.Count);
                var attribute = attributes[0];

                var args = attribute.GetConstructorArguments().ToList();
                Assert.NotNull(args);
                Assert.AreEqual(3, args.Count);
                Assert.AreEqual("hello", args[0]);
                Assert.AreEqual(42, args[1]);
            });
        }

        [Test]
        public void Should_return_named_arguments()
        {
            DoTest("Foo.HasNamedArgs", attributes =>
            {
                Assert.AreEqual(1, attributes.Count);
                var attribute = attributes[0];

                Assert.AreEqual("hello", attribute.GetNamedArgument<string>("StringProperty"));
                Assert.AreEqual("world", attribute.GetNamedArgument<string>("StringField"));
                Assert.AreEqual(42, attribute.GetNamedArgument<int>("IntProperty"));
                Assert.AreEqual(24, attribute.GetNamedArgument<int>("IntField"));
                Assert.Null(attribute.GetNamedArgument<string>("UnsetStringProperty"));
                Assert.AreEqual(0, attribute.GetNamedArgument<int>("UnsetIntProperty"));
            });
        }

        [Test]
        public void Should_return_custom_attributes()
        {
            DoTest("Foo.HasAttributed", attributes =>
            {
                Assert.AreEqual(1, attributes.Count);
                var attribute = attributes[0];

                Console.WriteLine(typeof(BaseAttribute).AssemblyQualifiedName);
                var customAttributes = attribute.GetCustomAttributes("Foo.BaseAttribute");
                Assert.NotNull(customAttributes);
                Assert.AreEqual(2, customAttributes.Count());
            });
        }
    }
}