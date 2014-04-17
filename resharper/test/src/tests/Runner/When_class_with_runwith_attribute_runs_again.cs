using System.Collections.Generic;
using System.Linq;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.Util;
using NUnit.Framework;
using XunitContrib.Runner.ReSharper.UnitTestProvider;

namespace XunitContrib.Runner.ReSharper.Tests.Runner
{
    [TestFixture("xunit1")]
    [TestFixture("xunit2")]
    public class When_class_with_runwith_attribute_runs_again : XunitTaskRunnerOutputTestBase
    {
        public When_class_with_runwith_attribute_runs_again(string environmentId)
            : base(environmentId)
        {
        }

        protected override string GetTestName()
        {
            return "HasRunWithKnownMethodTask";
        }

        protected override ICollection<IUnitTestElement> GetUnitTestElements(IProject testProject, string assemblyLocation)
        {
            var elements = base.GetUnitTestElements(testProject, assemblyLocation).ToList();
            var element = elements.Single(e => e.ShortName == "HasRunWith") as XunitTestClassElement;

            var testProvider = testProject.GetComponent<XunitTestProvider>();
            var declaredElementProvider = testProject.GetComponent<DeclaredElementProvider>();
            var theoryElement = UnitTestElementFactory.CreateTestMethod(testProvider, testProject,
                declaredElementProvider, element, element.TypeName, "TestMethodWithExistingTask",
                string.Empty, EmptyArray<UnitTestElementCategory>.Instance, isDynamic: true);
            elements.Add(theoryElement);
            return elements;
        }

        [Test]
        public void Should_reuse_existing_dynamic_method_task()
        {
            TaskId testMethodWithExistingTaskId = ForTaskOnly("Foo.HasRunWith", "TestMethodWithExistingTask");
            AssertDoesNotContain(testMethodWithExistingTaskId, TaskAction.Create);
            AssertContainsStart(testMethodWithExistingTaskId);
        }
    }
}