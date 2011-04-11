using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Xml;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.TaskRunnerFramework;
using JetBrains.ReSharper.TaskRunnerFramework.UnitTesting;
using JetBrains.ReSharper.UnitTestFramework;
using XunitContrib.Runner.ReSharper.RemoteRunner;
using XunitContrib.Runner.ReSharper.UnitTestProvider.Properties;
using XunitContrib.Runner.ReSharper.UnitTestRunnerProvider;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    using ReadFromXmlFunc = Func<XmlElement, IUnitTestViewElement, XunitTestProvider, IUnitTestViewElement>;

    [UnitTestProvider, UsedImplicitly]
    public class XunitTestProvider : XunitTestRunnerProvider, IUnitTestProvider
    {
        private static readonly AssemblyLoader AssemblyLoader = new AssemblyLoader();
        private static readonly UnitTestElementComparer Comparer = new UnitTestElementComparer(new[] { typeof(XunitViewTestClassElement), typeof(XunitViewTestMethodElement) });

        private static readonly IDictionary<string, ReadFromXmlFunc> DeserialiseMap = new Dictionary<string, ReadFromXmlFunc>
                {
                    {typeof (XunitViewTestClassElement).Name, XunitViewTestClassElement.ReadFromXml},
                    {typeof (XunitViewTestMethodElement).Name, XunitViewTestMethodElement.ReadFromXml}
                };

        static XunitTestProvider()
        {
            // ReSharper automatically adds all test providers to the list of assemblies it uses
            // to handle the AppDomain.Resolve event

            // The test runner process talks to devenv/resharper via remoting, and so devenv needs
            // to be able to resolve the remote assembly to be able to recreate the serialised types.
            // (Aside: the assembly is already loaded - why does it need to resolve it again?)
            // ReSharper automatically adds all unit test provider assemblies to the list of assemblies
            // it uses to handle the AppDomain.Resolve event. Since we've got a second assembly to
            // live in the remote process, we need to add this to the list.
            //AssemblyLoader.RegisterAssembly(typeof(XunitTaskRunner).Assembly);
        }

        public XunitTestProvider(ISolution solution, [Optional, DefaultParameterValue(null)] CacheManager cacheManager,
            [Optional, DefaultParameterValue(null)] PsiModuleManager psiModuleManager,
            [Optional, DefaultParameterValue(null)] UnitTestingCategoriesProvider categoriesProvider)
        {
            Solution = solution;
            CacheManager = cacheManager;
            PsiModuleManager = psiModuleManager;

            var unitTestingAssemblyLoader = solution.GetComponent<UnitTestingAssemblyLoader>();
            unitTestingAssemblyLoader.RegisterAssembly(typeof(XunitTaskRunner).Assembly);
        }

        public IUnitTestViewElement DeserializeElement(XmlElement parent, IUnitTestViewElement parentElement)
        {
            if (!parent.HasAttribute("type"))
                throw new ArgumentException("Element is not xunit");

            Func<XmlElement, IUnitTestViewElement, XunitTestProvider, IUnitTestViewElement> func;
            if (DeserialiseMap.TryGetValue(parent.GetAttribute("type"), out func))
                return func(parent, parentElement, this);

            throw new ArgumentException("Element is not xunit");
        }

        public Image Icon
        {
            get { return Resources.xunit; }
        }

        public ISolution Solution { get; private set; }

        public bool IsSupported(IHostProvider hostProvider)
        {
            return true;
        }

        public int CompareUnitTestElements(IUnitTestElement x, IUnitTestElement y)
        {
            return Comparer.Compare(x, y);
        }

        public void SerializeElement(XmlElement parent, IUnitTestElement element)
        {
            parent.SetAttribute("type", element.GetType().Name);

            var writableUnitTestElement = (ISerializableUnitTestElement)element;
            writableUnitTestElement.WriteToXml(parent);
        }

        public void ExploreExternal(UnitTestElementConsumer consumer)
        {
            // Called from a refresh of the Unit Test Explorer
            // Allows us to explore anything that's not a part of the solution + projects world
        }

        public void ExploreFile(IFile psiFile, UnitTestElementLocationConsumer consumer, CheckForInterrupt interrupted)
        {
            if (psiFile == null)
                throw new ArgumentNullException("psiFile");

            psiFile.ProcessDescendants(new XunitPsiFileExplorer(this, consumer, psiFile, interrupted));

#if DEBUG
            UnitTestManager.GetInstance(Solution).EnableDebugInternal = true;
#endif
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
                    return !UnitTestElementIdentifier.IsAnyUnitTestElement(declaredElement);

                case UnitTestElementKind.Test:
                    return UnitTestElementIdentifier.IsUnitTest(declaredElement);

                case UnitTestElementKind.TestContainer:
                    return UnitTestElementIdentifier.IsUnitTestContainer(declaredElement);

                case UnitTestElementKind.TestStuff:
                    return UnitTestElementIdentifier.IsUnitTestStuff(declaredElement);
            }

            return false;
        }

        public bool IsElementOfKind(IUnitTestElement element, UnitTestElementKind elementKind)
        {
            switch (elementKind)
            {
                case UnitTestElementKind.Unknown:
                    return !(element is XunitViewTestMethodElement || element is XunitViewTestClassElement);

                case UnitTestElementKind.Test:
                    return element is XunitViewTestMethodElement;

                case UnitTestElementKind.TestContainer:
                    return element is XunitViewTestClassElement;

                case UnitTestElementKind.TestStuff:
                    return element is XunitViewTestMethodElement || element is XunitViewTestClassElement;
            }

            return false;
        }

        internal PsiModuleManager PsiModuleManager { get; private set; }
        internal CacheManager CacheManager { get; private set; }

        internal XunitViewTestClassElement GetOrCreateTestClass(string id, IProject project, string typeName, string methodName, string assemblyLocation)
        {
            var element = UnitTestManager.GetInstance(Solution).GetOrCreateElementById(project, id,
                () => new XunitViewTestClassElement(this, project, typeName, methodName, assemblyLocation));
            if (element != null)
            {
                element.State = UnitTestElementState.Valid;
                return element as XunitViewTestClassElement;
            }

            return new XunitViewTestClassElement(this, project, typeName, methodName, assemblyLocation);
        }

        internal XunitViewTestMethodElement GetOrCreateTestMethod(string id, IProject project, XunitViewTestClassElement parent, string typeName, string methodName, bool isSkip)
        {
            var element = UnitTestManager.GetInstance(Solution).GetOrCreateElementById(project, id,
                () => new XunitViewTestMethodElement(this, parent, project, id, typeName, methodName, isSkip));
            if (element != null)
            {
                element.Parent = parent;
                element.State = UnitTestElementState.Valid;
                return element as XunitViewTestMethodElement;
            }

            return new XunitViewTestMethodElement(this, parent, project, id, typeName, methodName, isSkip);
        }
    }
}
