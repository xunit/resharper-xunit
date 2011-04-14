using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.Metadata.Utils;
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
        private static readonly UnitTestElementComparer Comparer = new UnitTestElementComparer(new[] { typeof(XunitViewTestClassElement), typeof(XunitViewTestMethodElement) });

        private static readonly IDictionary<string, ReadFromXmlFunc> DeserialiseMap = new Dictionary<string, ReadFromXmlFunc>
                {
                    {typeof (XunitViewTestClassElement).Name, XunitViewTestClassElement.ReadFromXml},
                    {typeof (XunitViewTestMethodElement).Name, XunitViewTestMethodElement.ReadFromXml}
                };

        public XunitTestProvider(ISolution solution, CacheManager cacheManager, PsiModuleManager psiModuleManager, UnitTestingCategoriesProvider categoriesProvider)
        {
            Solution = solution;
            CacheManager = cacheManager;
            PsiModuleManager = psiModuleManager;

            var unitTestingAssemblyLoader = solution.GetComponent<UnitTestingAssemblyLoader>();
            unitTestingAssemblyLoader.RegisterAssembly(typeof(XunitTaskRunner).Assembly);
        }

        public override RemoteTaskRunnerInfo GetTaskRunnerInfo()
        {
#if DEBUG
            // Causes the external test runner to display a message box before running, very handy for attaching the debugger
            // and while it's a bit crufty here, we know this method gets called before a test run
            UnitTestManager.GetInstance(Solution).EnableDebugInternal = true;
#endif

            return base.GetTaskRunnerInfo();
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

            var project = psiFile.GetProject();
            if (project == null)
                return;

            if (!project.GetAssemblyReferences().Any(IsSilverlightMscorlib))
                return;

            psiFile.ProcessDescendants(new XunitPsiFileExplorer(this, consumer, psiFile, interrupted));
        }

        private static bool IsSilverlightMscorlib(IProjectToAssemblyReference x)
        {
            var assemblyNameInfo = x.ReferenceTarget.AssemblyName;
            var publicKeyToken = AssemblyNameExtensions.GetPublicKeyTokenString(assemblyNameInfo.GetPublicKeyToken());

            // Not sure if this is the best way to do this, but the public key token for mscorlib on
            // the desktop if "b77a5c561934e089". On Silverlight, it's "7cec85d7bea7798e"
            return assemblyNameInfo.Name=="mscorlib" && publicKeyToken == "b77a5c561934e089";
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
