using System;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.Properties.Managed;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;
using Xunit.Abstractions;
using XunitContrib.Runner.ReSharper.UnitTestProvider;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Abstractions
{
    public class PsiAssemblyInfoAdapterTest : BaseTestWithSingleProject
    {
        protected override string RelativeTestDataPath
        {
            get { return "Abstractions"; }
        }

        private void DoTest(Action<IProject, IAssemblyInfo> action)
        {
            WithSingleProject("Foo", (lifetime, solution, project) =>
            {
                RunGuarded(() =>
                {
                    var configuration = (IManagedProjectConfiguration)project.ProjectProperties.ActiveConfiguration;
                    Assert.NotNull(configuration);

                    configuration.RelativeOutputDirectory = @"bin\Debug";
                });

                var assembly = new PsiAssemblyInfoAdapter(project);
                action(project, assembly);
            });
        }

        [Test]
        public void Should_return_assembly_path()
        {
            DoTest((project, info) => StringAssert.EndsWith(@"\bin\Debug\TestProject.dll", info.AssemblyPath));
        }

        [Test]
        public void Should_return_name()
        {
            // TODO: Return full assembly qualified name
            // e.g. "TestProject, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            // This requires looking at assembly attributes that could be ANYWHERE in the project...
            DoTest((project, info) => Assert.AreEqual("TestProject", info.Name));
        }

        [Test, Ignore("OMG. This requires an assembly attribute cache")]
        public void Should_return_custom_attributes()
        {
        }

        [Test]
        public void Should_return_specific_type_info()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void Should_return_public_types()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void Should_return_public_nested_types()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void Should_return_private_nested_types()
        {
            throw new NotImplementedException();
        }
    }
}