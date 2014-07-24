using System;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.Properties.Managed;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Abstractions
{
    public abstract class AbstractionsSourceBaseTest : BaseTestWithSingleProject
    {
        protected override string RelativeTestDataPath
        {
            get { return "Abstractions"; }
        }

        protected abstract string Filename { get; }

        protected void DoTest(Action<IProject> action)
        {
            WithSingleProject(Filename, (lifetime, solution, project) => RunGuarded(() =>
            {
                var configuration = (IManagedProjectConfiguration)project.ProjectProperties.ActiveConfiguration;
                Assert.NotNull(configuration);

                configuration.RelativeOutputDirectory = @"bin\Debug";

                action(project);
            }));
        }
    }
}