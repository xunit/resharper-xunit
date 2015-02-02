using System.Collections.Generic;
using System.Linq;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.UnitTestFramework;
using NUnit.Framework;
using XunitContrib.Runner.ReSharper.UnitTestProvider;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Runner
{
    [TestFixture("xunit1", Category = "xunit1")]
    [TestFixture("xunit2", Category = "xunit2")]
    public class When_a_theory_reuses_existing_task : XunitTaskRunnerOutputTestBase
    {
        public When_a_theory_reuses_existing_task(string environment)
            : base(environment)
        {
        }

        protected override string GetTestName()
        {
            return "TheoryWithExistingTask";
        }

        protected override ICollection<IUnitTestElement> GetUnitTestElements(IProject testProject, string assemblyLocation)
        {
            var elements = base.GetUnitTestElements(testProject, assemblyLocation).ToList();
            var unitTestElementFactory = testProject.GetComponent<UnitTestElementFactory>();
            var theoryElement = unitTestElementFactory.GetOrCreateTestTheory(testProject,
                (XunitTestMethodElement) elements[1], "TestMethod(value: 42)");
            elements.Add(theoryElement);
            return elements;
        }

        [Test]
        public void Should_reuse_existing_theory_task()
        {
            var theoryTask = ForTaskOnly("Foo.Theory", "TestMethod", "TestMethod(value: 42)");
            AssertDoesNotContain(theoryTask, TaskAction.Create);
            AssertContainsStart(theoryTask);
        }
    }
}