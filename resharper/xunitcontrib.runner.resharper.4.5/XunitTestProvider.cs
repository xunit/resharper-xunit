using System;
using System.Collections.Generic;
using System.Drawing;
using JetBrains.Application;
using JetBrains.CommonControls;
using JetBrains.Metadata.Access;
using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.TaskRunnerFramework;
using JetBrains.ReSharper.UnitTestExplorer;
using JetBrains.TreeModels;
using JetBrains.UI.TreeView;
using JetBrains.Util;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper
{
    [UnitTestProvider]
    class XunitTestProvider : IUnitTestProvider
    {
        static readonly XunitBrowserPresenter presenter = new XunitBrowserPresenter();

        public string ID
        {
            get { return "xUnit"; }
        }

        public string Name
        {
            get { return "xUnit.net"; }
        }

        public Image Icon
        {
            get { return null; }
        }

        public int CompareUnitTestElements(UnitTestElement x,
                                           UnitTestElement y)
        {
            if (Equals(x, y))
                return 0;

            int compare = StringComparer.CurrentCultureIgnoreCase.Compare(x.GetTypeClrName(), y.GetTypeClrName());
            if (compare != 0)
                return compare;

            if (x is XunitTestElementMethod && y is XunitTestElementClass)
                return -1;

            if (x is XunitTestElementClass && y is XunitTestElementMethod)
                return 1;

            if (x is XunitTestElementClass && y is XunitTestElementClass)
                return 0;

            XunitTestElementMethod xe = (XunitTestElementMethod)x;
            XunitTestElementMethod ye = (XunitTestElementMethod)y;
            return xe.Order.CompareTo(ye.Order);
        }

        public UnitTestElement Deserialize(ISolution solution,
                                           string elementString)
        {
            return null;
        }

        public void ExploreAssembly(IMetadataAssembly assembly,
                                    IProject project,
                                    UnitTestElementConsumer consumer)
        {
            if(!ReferencesXUnit(assembly))
                return;

            ExploreTypes(assembly.GetTypes(), assembly, project, consumer);
        }

        private void ExploreTypes(IEnumerable<IMetadataTypeInfo> types, IMetadataAssembly assembly, IProjectModelElement project, UnitTestElementConsumer consumer)
        {
            foreach (IMetadataTypeInfo type in types)
            {
                IMetadataTypeInfo[] nestedTypes = type.GetNestedTypes();
                if (nestedTypes != null)
                    ExploreTypes(nestedTypes, assembly, project, consumer);

                ITypeInfo typeInfo = TypeWrapper.Wrap(type);
                if (!IsPublic(type) || !TypeUtility.IsTestClass(typeInfo) || TypeUtility.HasRunWith(typeInfo))
                    continue;

                ITestClassCommand command = TestClassCommandFactory.Make(typeInfo);
                if (command == null)
                    continue;

                ExploreTestClass(assembly, type, command.EnumerateTestMethods(), project, consumer);
            }
        }

        private static bool IsPublic(IMetadataTypeInfo type)
        {
            // Hmmm. This seems a little odd. Resharper reports public nested types with IsNestedPublic,
            // while IsPublic is false
            return (type.IsNested && type.IsNestedPublic) || type.IsPublic;
        }

        private static bool ReferencesXUnit(IMetadataAssembly assembly)
        {
            foreach (AssemblyReference reference in assembly.ReferencedAssembliesNames)
            {
                if(reference.AssemblyName.Name.ToLowerInvariant() == "xunit")
                    return true;
            }

            return false;
        }

        public ProviderCustomOptionsControl GetCustomOptionsControl(ISolution solution)
        {
            return null;
        }

        public void ExploreExternal(UnitTestElementConsumer consumer)
        {
            throw new NotImplementedException();
        }

        public void ExploreFile(IFile psiFile,
                                UnitTestElementLocationConsumer consumer,
                                CheckForInterrupt interrupted)
        {
            if (psiFile == null)
                throw new ArgumentNullException("psiFile");

            psiFile.ProcessDescendants(new XunitFileExplorer(this, consumer, psiFile, interrupted));
        }

        public void ExploreSolution(ISolution solution, UnitTestElementConsumer consumer)
        {
        }

        void ExploreTestClass(IMetadataAssembly assembly,
                              IMetadataTypeInfo type,
                              IEnumerable<IMethodInfo> methods,
                              IProjectModelElement project,
                              UnitTestElementConsumer consumer)
        {
            XunitTestElementClass @class = new XunitTestElementClass(this, project, type.FullyQualifiedName, assembly.Location);
            consumer(@class);

            int order = 0;

            foreach (IMethodInfo method in methods)
                if (MethodUtility.IsTest(method))
                    ExploreTestMethod(project, @class, consumer, method, order++);
        }

        void ExploreTestMethod(IProjectModelElement project,
                               XunitTestElementClass @class,
                               UnitTestElementConsumer consumer,
                               IMethodInfo method,
                               int order)
        {
            XunitTestElementMethod test = new XunitTestElementMethod(this, @class, project, method.DeclaringTypeName, method.Name, order);
            test.SetExplicit(MethodUtility.GetSkipReason(method));
            consumer(test);
        }

        public RemoteTaskRunnerInfo GetTaskRunnerInfo()
        {
            return new RemoteTaskRunnerInfo(typeof(XunitRunner));
        }

        public IList<UnitTestTask> GetTaskSequence(UnitTestElement element,
                                                   IList<UnitTestElement> explicitElements)
        {
            XunitTestElementMethod testMethod = element as XunitTestElementMethod;

            if (testMethod != null)
            {
                XunitTestElementClass testClass = testMethod.Class;
                List<UnitTestTask> result = new List<UnitTestTask>();

                result.Add(new UnitTestTask(null,
                                            new AssemblyLoadTask(testClass.AssemblyLocation)));

                result.Add(new UnitTestTask(testClass,
                                            new XunitTestClassTask(testClass.AssemblyLocation,
                                                                   testClass.GetTypeClrName(),
                                                                   explicitElements.Contains(testClass))));

                result.Add(new UnitTestTask(testMethod,
                                            new XunitTestMethodTask(testClass.GetTypeClrName(),
                                                                    testMethod.MethodName,
                                                                    explicitElements.Contains(testMethod))));

                return result;
            }

            if (element is XunitTestElementClass)
                return EmptyArray<UnitTestTask>.Instance;

            throw new ArgumentException(string.Format("element is not xUnit: '{0}'", element));
        }

        public bool IsUnitTestElement(IDeclaredElement element)
        {
            IClass testClass = element as IClass;
            if (testClass != null && TypeUtility.IsTestClass(TypeWrapper.Wrap(testClass)))
                return true;

            IMethod testMethod = element as IMethod;
            if (testMethod != null && MethodUtility.IsTest(MethodWrapper.Wrap(testMethod)))
                return true;

            return false;
        }

        public bool IsUnitTestStuff(IDeclaredElement element)
        {
            if (element.ShortName == "PublicTestMethodOnPublicClassShouldNotBeFlagged")
            {
                int i = 0;
                i++;
            }

            return IsUnitTestElement(element);
        }

        public void Present(UnitTestElement element,
                            IPresentableItem presentableItem,
                            TreeModelNode node,
                            PresentationState state)
        {
            presenter.UpdateItem(element, node, presentableItem, state);
        }

        public void ProfferConfiguration(TaskExecutorConfiguration configuration,
                                         UnitTestSession session) {}

        public string Serialize(UnitTestElement element)
        {
            return null;
        }
    }
}