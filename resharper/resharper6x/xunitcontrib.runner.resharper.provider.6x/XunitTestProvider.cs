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

                // Make sure we return an element, even if the system already knows about it. If it's
                // part of the test run, it will already have been added in GetTaskSequence, and this
                // method (GetDynamicElement) doesn't get called. But if you try and run just a single
                // theory, xunit will run ALL theories for that method, and will need the elements
                // for those theories not included in the task sequence. This is necessary because if
                // one of those theories throws an exception, UnitTestLaunch.TaskException doesn't
                // have an element to report against, and displays a message box
                if (element != null)
                    return element;

                return UnitTestElementFactory.CreateTestTheory(this, project, methodElement, theoryTask.Name);
            }
        }
    }
}
