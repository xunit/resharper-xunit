using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.ReSharper.TaskRunnerFramework;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.Util;
using Xunit.Sdk;
using XunitContrib.Runner.ReSharper.RemoteRunner;
using XunitContrib.Runner.ReSharper.RemoteRunner.Tasks;

// ReSharper 8.2 doesn't have this namespace, used by 9.0
namespace JetBrains.ReSharper.Resources.Shell
{
}

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    [UnitTestProvider, UsedImplicitly]
    public partial class XunitTestProvider : IUnitTestProvider, IDynamicUnitTestProvider
    {
        private static readonly UnitTestElementComparer Comparer = new UnitTestElementComparer(typeof(XunitTestClassElement), typeof(XunitTestMethodElement), typeof(XunitTestTheoryElement));

        public static readonly IClrTypeName PropertyDataAttribute = new ClrTypeName("Xunit.Extensions.PropertyDataAttribute");

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

        // ReSharper 8.2
        public IUnitTestElement GetDynamicElement(RemoteTask remoteTask, Dictionary<RemoteTask, IUnitTestElement> tasks)
        {
            var taskIdsToElements = tasks.ToDictionary(x => x.Key.Id, x => x.Value);
            return GetDynamicElement(remoteTask, taskIdsToElements);
        }

        public IUnitTestElement GetDynamicElement(RemoteTask remoteTask, Dictionary<string, IUnitTestElement> taskIdsToElements)
        {
            var theoryTask = remoteTask as XunitTestTheoryTask;
            if (theoryTask != null)
                return GetDynamicTheoryElement(taskIdsToElements, theoryTask);

            var methodTask = remoteTask as XunitTestMethodTask;
            if (methodTask != null)
                return GetDynamicMethodElement(taskIdsToElements, methodTask);

            return null;
        }

        private IUnitTestElement GetDynamicTheoryElement(Dictionary<string, IUnitTestElement> tasks, XunitTestTheoryTask theoryTask)
        {
            IUnitTestElement parentElement;
            if (!tasks.TryGetValue(theoryTask.UncollapsedParentTaskId, out parentElement))
                return null;

            var methodElement = parentElement as XunitTestMethodElement;
            if (methodElement == null)
                return null;

            using (ReadLockCookie.Create())
            {
                var project = methodElement.GetProject();
                if (project == null)
                    return null;

                var unitTestElementFactory = project.GetSolution().GetComponent<UnitTestElementFactory>();

                // Make sure we return an element, even if the system already knows about it. If it's
                // part of the test run, it will already have been added in GetTaskSequence, and this
                // method (GetDynamicElement) doesn't get called. But if you try and run just a single
                // theory, xunit will run ALL theories for that method, and will need the elements
                // for those theories not included in the task sequence. This is necessary because if
                // one of those theories throws an exception, UnitTestLaunch.TaskException doesn't
                // have an element to report against, and displays a message box
                var element = unitTestElementFactory.GetOrCreateTestTheory(project, methodElement, theoryTask.TheoryName);

                element.State = UnitTestElementState.Dynamic;
                element.SetCategories(parentElement.Categories);

                return element;
            }
        }

        private IUnitTestElement GetDynamicMethodElement(Dictionary<string, IUnitTestElement> tasks, XunitTestMethodTask methodTask)
        {
            IUnitTestElement parentElement;
            if (!tasks.TryGetValue(methodTask.UncollapsedParentTaskId, out parentElement))
                return null;

            var classElement = parentElement as XunitTestClassElement;
            if (classElement == null)
                return null;

            using (ReadLockCookie.Create())
            {
                var project = classElement.GetProject();
                if (project == null)
                    return null;

                var unitTestElementFactory = project.GetSolution().GetComponent<UnitTestElementFactory>();

                // As for theories, make sure we always return an element
                var element = unitTestElementFactory.GetOrCreateTestMethod(project, classElement,
                    new ClrTypeName(methodTask.TypeName), methodTask.MethodName, string.Empty,
                    new OneToSetMap<string, string>(), isDynamic: true);

                element.State = UnitTestElementState.Dynamic;

                return element;
            }
        }
    }
}
