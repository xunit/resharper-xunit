using System.Collections.Generic;
using System.Linq;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.Util;
using NUnit.Framework;
using XunitContrib.Runner.ReSharper.UnitTestProvider;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Runner
{
    [Category("xunit1")]
    public class When_class_with_runwith_attribute_runs_again_xunit1 : XunitTaskRunnerOutputTestBase
    {
        // xunit2 doesn't support RunWith
        public When_class_with_runwith_attribute_runs_again_xunit1()
            : base("xunit1")
        {
        }

        protected override string GetTestName()
        {
            return "HasRunWithKnownMethodTask.xunit1";
        }

        protected override ICollection<IUnitTestElement> GetUnitTestElements(IProject testProject, string assemblyLocation)
        {
            var elements = base.GetUnitTestElements(testProject, assemblyLocation).ToList();
            var element = elements.Single(e => e.ShortName == "HasRunWith") as XunitTestClassElement;

            var unitTestElementFactory = testProject.GetComponent<UnitTestElementFactory>();
            var theoryElement = unitTestElementFactory.GetOrCreateTestMethod(testProject,
                element, element.TypeName, "TestMethodWithExistingTask",
                string.Empty, new OneToSetMap<string, string>(), isDynamic: true);
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