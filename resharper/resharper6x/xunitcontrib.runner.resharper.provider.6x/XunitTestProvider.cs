using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.TaskRunnerFramework;
using JetBrains.ReSharper.UnitTestFramework;
using XunitContrib.Runner.ReSharper.RemoteRunner;
using XunitContrib.Runner.ReSharper.UnitTestProvider.Properties;
using System.Linq;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    [UnitTestProvider, UsedImplicitly]
    public partial class XunitTestProvider : IUnitTestProvider, IDynamicUnitTestProvider
    {
        private static readonly UnitTestElementComparer Comparer = new UnitTestElementComparer(new[]
                                                                                                   {
                                                                                                       typeof(XunitTestClassElement),
                                                                                                       typeof(XunitTestMethodElement),
                                                                                                       typeof(XunitTestTheoryElement)
                                                                                                   });

        public RemoteTaskRunnerInfo GetTaskRunnerInfo()
        {
#if DEBUG
            // Causes the external test runner to display a message box before running, very handy for attaching the debugger
            // and while it's a bit crufty here, we know this method gets called before a test run
            //settings.EnableDebugInternal = true;
#endif

            return new RemoteTaskRunnerInfo(typeof(XunitTaskRunner));
        }

        public Image Icon
        {
            get { return Resources.xunit; }
        }

        public string ID
        {
            get { return XunitTestRunner.RunnerId; }
        }

        public string Name
        {
            get { return "xunit.net"; }
        }

        public bool IsSupported(IHostProvider hostProvider)
        {
            return true;
        }

        public int CompareUnitTestElements(IUnitTestElement x, IUnitTestElement y)
        {
            return Comparer.Compare(x, y);
        }

        public void ExploreExternal(UnitTestElementConsumer consumer)
        {
            // Called from a refresh of the Unit Test Explorer
            // Allows us to explore anything that's not a part of the solution + projects world
        }

        public void ExploreSolution(ISolution solution, UnitTestElementConsumer consumer)
        {
            // Called from a refresh of the Unit Test Explorer
            // Allows us to explore the solution, without going into the projects
        }

        // Used to discover the type of the element - unknown, test, test container (class) or
        // something else relating to a test element (e.g. parent class of a nested test class)
        // This method is called to get the icon for the completion lists, amongst other things
        public bool IsElementOfKind(IDeclaredElement declaredElement, UnitTestElementKind elementKind)
        {
            switch (elementKind)
            {
                case UnitTestElementKind.Unknown:
                    return !declaredElement.IsAnyUnitTestElement();

                case UnitTestElementKind.Test:
                    return declaredElement.IsUnitTest();

                case UnitTestElementKind.TestContainer:
                    return declaredElement.IsUnitTestContainer();

                case UnitTestElementKind.TestStuff:
                    return declaredElement.IsUnitTestStuff();
            }

            return false;
        }

        public bool IsElementOfKind(IUnitTestElement element, UnitTestElementKind elementKind)
        {
            switch (elementKind)
            {
                case UnitTestElementKind.Unknown:
                    return !(element is XunitTestMethodElement || element is XunitTestClassElement);

                case UnitTestElementKind.Test:
                    return element is XunitTestMethodElement;

                case UnitTestElementKind.TestContainer:
                    return element is XunitTestClassElement || element is XunitInheritedTestMethodContainerElement;

                case UnitTestElementKind.TestStuff:
                    return element is XunitTestMethodElement || element is XunitTestClassElement;
            }

            return false;
        }

        public IUnitTestElement GetDynamicElement(RemoteTask remoteTask, Dictionary<RemoteTask, IUnitTestElement> tasks)
        {
            var theoryTask = remoteTask as XunitTestTheoryTask;
            if (theoryTask == null)
                return null;

            var methodElement = (from kvp in tasks
                                 where kvp.Key is XunitTestMethodTask && ((XunitTestMethodTask) kvp.Key).ElementId == theoryTask.ParentElementId
                                 select kvp.Value).FirstOrDefault() as XunitTestMethodElement;
            if (methodElement == null)
                return null;

            using(ReadLockCookie.Create())
            {
                var project = methodElement.GetProject();
                if (project == null)
                    return null;

                var element = UnitTestElementFactory.GetTestTheory(project, methodElement, theoryTask.Name);

                // If it exists, it should have already been passed to the test framework, so should
                // be well known, and we shouldn't need to create a new one
                if (element != null)
                    return null;

                return UnitTestElementFactory.CreateTestTheory(this, project, methodElement, theoryTask.Name);
            }
        }
    }
}
