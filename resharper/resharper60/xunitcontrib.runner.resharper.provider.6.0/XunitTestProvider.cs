using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using JetBrains.Annotations;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.TaskRunnerFramework;
using JetBrains.ReSharper.UnitTestFramework;
using XunitContrib.Runner.ReSharper.RemoteRunner;
using XunitContrib.Runner.ReSharper.UnitTestProvider.Properties;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    using ReadFromXmlFunc = Func<XmlElement, IUnitTestElement, XunitTestProvider, IUnitTestElement>;

    [UnitTestProvider, UsedImplicitly]
    public class XunitTestProvider : IUnitTestProvider
    {
        private static readonly UnitTestElementComparer Comparer = new UnitTestElementComparer(new[] { typeof(XunitTestClassElement), typeof(XunitTestMethodElement) });

        private static readonly IDictionary<string, ReadFromXmlFunc> DeserialiseMap = new Dictionary<string, ReadFromXmlFunc>
                {
                    {typeof (XunitTestClassElement).Name, XunitTestClassElement.ReadFromXml},
                    {typeof (XunitTestMethodElement).Name, XunitTestMethodElement.ReadFromXml}
                };

        public XunitTestProvider(ISolution solution, CacheManager cacheManager, PsiModuleManager psiModuleManager, UnitTestingCategoriesProvider categoriesProvider)
        {
            Solution = solution;
            CacheManager = cacheManager;
            PsiModuleManager = psiModuleManager;

            var unitTestingAssemblyLoader = solution.GetComponent<UnitTestingAssemblyLoader>();
            unitTestingAssemblyLoader.RegisterAssembly(typeof(XunitTaskRunner).Assembly);
        }

        public RemoteTaskRunnerInfo GetTaskRunnerInfo()
        {
#if DEBUG
            // Causes the external test runner to display a message box before running, very handy for attaching the debugger
            // and while it's a bit crufty here, we know this method gets called before a test run
            UnitTestManager.GetInstance(Solution).EnableDebugInternal = true;
#endif

            return new RemoteTaskRunnerInfo(typeof(XunitTaskRunner));
        }

        public void SerializeElement(XmlElement parent, IUnitTestElement element)
        {
            parent.SetAttribute("type", element.GetType().Name);

            var writableUnitTestElement = (ISerializableUnitTestElement)element;
            writableUnitTestElement.WriteToXml(parent);
        }

        public IUnitTestElement DeserializeElement(XmlElement parent, IUnitTestElement parentElement)
        {
            if (!parent.HasAttribute("type"))
                throw new ArgumentException("Element is not xunit");

            Func<XmlElement, IUnitTestElement, XunitTestProvider, IUnitTestElement> func;
            if (DeserialiseMap.TryGetValue(parent.GetAttribute("type"), out func))
                return func(parent, parentElement, this);

            throw new ArgumentException("Element is not xunit");
        }

        public Image Icon
        {
            get { return Resources.xunit; }
        }

        public ISolution Solution { get; private set; }

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
                    return element is XunitTestClassElement;

                case UnitTestElementKind.TestStuff:
                    return element is XunitTestMethodElement || element is XunitTestClassElement;
            }

            return false;
        }

        internal PsiModuleManager PsiModuleManager { get; private set; }
        internal CacheManager CacheManager { get; private set; }

        internal XunitTestClassElement GetOrCreateTestClass(IProject project, IClrTypeName typeName, string assemblyLocation)
        {
            var id = typeName.FullName;
            var element = UnitTestManager.GetInstance(Solution).GetElementById(project, id);
            if (element != null)
                return element as XunitTestClassElement;

            return new XunitTestClassElement(this, new ProjectModelElementEnvoy(project), typeName.GetPersistent(), assemblyLocation);
        }

        internal XunitTestMethodElement GetOrCreateTestMethod(IProject project, XunitTestClassElement parent, IClrTypeName typeName, string methodName, string skipReason)
        {
            var id = typeName.FullName + "." + methodName;
            var element = UnitTestManager.GetInstance(Solution).GetElementById(project, id);
            if (element != null)
            {
                element.State = UnitTestElementState.Valid;
                return element as XunitTestMethodElement;
            }

            return new XunitTestMethodElement(this, parent, new ProjectModelElementEnvoy(project), id, typeName, methodName, skipReason);
        }
    }
}
