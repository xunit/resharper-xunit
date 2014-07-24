using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using NUnit.Framework;
using Xunit.Abstractions;
using XunitContrib.Runner.ReSharper.UnitTestProvider;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Abstractions
{
    public class PsiAttributeInfoAdapterTest : AbstractionsSourceBaseTest
    {
        protected override string Filename
        {
            get { return "Abstractions.cs"; }
        }

        private void DoTest(string type, Action<IEnumerable<IAttributeInfo>> action)
        {
            DoTest(project =>
            {
                var psiServices = project.GetSolution().GetComponent<IPsiServices>();
                var module = psiServices.Modules.GetPrimaryPsiModule(project);
                var symbolScope = psiServices.Symbols.GetSymbolScope(module, project.GetResolveContext(), false, true);
                var typeElement = symbolScope.GetTypeElementByCLRName(type);
                Assert.NotNull(typeElement);
                var attributes = from attribute in typeElement.GetAttributeInstances(true)
                    select new PsiAttributeInfoAdapter2(attribute);
                action(attributes);
            });
        }

        [Test, Ignore("Not yet implemented")]
        public void Should_return_constructor_arguments()
        {
            DoTest("Foo.HasCtorArgs", attributes =>
            {
                
            });
        }

        [Test, Ignore("Not yet implemented")]
        public void Should_return_named_arguments()
        {
            DoTest("Foo.HasNamedArgs", attributes =>
            {

            });
        }

        [Test, Ignore("Not yet implemented")]
        public void Should_return_custom_attributes()
        {
            DoTest("Foo.HasDecorated", attributes =>
            {

            });
        }
    }
}