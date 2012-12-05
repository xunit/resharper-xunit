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

        public static readonly ClrTypeName PropertyDataAttribute = new ClrTypeName("Xunit.Extensions.PropertyDataAttribute");

        public RemoteTaskRunnerInfo GetTaskRunnerInfo()
        {
            return new RemoteTaskRunnerInfo(typeof(XunitTaskRunner));
        }

        public Image Icon
        {
            get { return Resources.xunit; }
        }

        public string ID
        {
            get { return XunitTaskRunner.RunnerId; }
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
                    return declaredElement.IsAnyUnitTestElement();
            }

            return false;
        }

        public bool IsElementOfKind(IUnitTestElement element, UnitTestElementKind elementKind)
        {
            switch (elementKind)
            {
                case UnitTestElementKind.Unknown:
                    return !(element is XunitTestMethodElement || element is XunitTestClassElement || element is XunitTestTheoryElement);

                case UnitTestElementKind.Test:
                    return (element is XunitTestMethodElement && !element.Children.Any()) || element is XunitTestTheoryElement;

                case UnitTestElementKind.TestContainer:
                    return element is XunitTestClassElement || element is XunitInheritedTestMethodContainerElement || (element is XunitTestMethodElement && element.Children.Any());

                case UnitTestElementKind.TestStuff:
                    return element is XunitTestMethodElement || element is XunitTestClassElement || element is XunitInheritedTestMethodContainerElement || element is XunitTestTheoryElement;
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
                {
                    // If the element is invalid, it's been removed from its parent, so add it back,
                    // and reset the state
                    if (element.State == UnitTestElementState.Invalid)
                    {
                        element.State = UnitTestElementState.Dynamic;
                        element.Parent = methodElement;
                    }
                    return element;
                }

                return UnitTestElementFactory.CreateTestTheory(this, project, methodElement, theoryTask.Name);
            }
        }
    }
}
