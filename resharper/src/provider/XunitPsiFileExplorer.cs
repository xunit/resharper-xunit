using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI;
using JetBrains.ReSharper.Psi.Search;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.Util;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class XunitPsiFileExplorer : IRecursiveElementProcessor
    {
        private readonly UnitTestElementFactory unitTestElementFactory;
        private readonly IUnitTestElementsObserver observer;
        private readonly IFile file;
        private readonly Func<bool> interrupted;
        private readonly SearchDomainFactory searchDomainFactory;
        private readonly IProject project;
        private readonly string assemblyPath;
        private readonly IDictionary<ITypeElement, XunitTestClassElement> classes = new Dictionary<ITypeElement, XunitTestClassElement>();
        private readonly IDictionary<ITypeElement, IList<XunitTestClassElement>> derivedTestClassElements = new Dictionary<ITypeElement, IList<XunitTestClassElement>>(); 
        private readonly IProjectFile projectFile;

        // TODO: The nunit code uses UnitTestAttributeCache
        public XunitPsiFileExplorer(XunitTestProvider provider, UnitTestElementFactory unitTestElementFactory,
                                    IUnitTestElementsObserver observer, IFile file, 
                                    Func<bool> interrupted, SearchDomainFactory searchDomainFactory)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            if (provider == null)
                throw new ArgumentNullException("provider");

            this.observer = observer;
            this.unitTestElementFactory = unitTestElementFactory;
            this.file = file;
            this.interrupted = interrupted;
            this.searchDomainFactory = searchDomainFactory;
            projectFile = file.GetSourceFile().ToProjectFile();
            project = file.GetProject();
            assemblyPath = project.GetOutputFilePath().FullPath;
        }

        public bool ProcessingIsFinished
        {
            get
            {
                if (interrupted())
                    throw new ProcessCancelledException();

                return false;
            }
        }

        public bool InteriorShouldBeProcessed(ITreeNode element)
        {
            if (element is ITypeMemberDeclaration)
                return (element is ITypeDeclaration);

            return true;
        }

        public void ProcessBeforeInterior(ITreeNode element)
        {
            var declaration = element as IDeclaration;
            if (declaration == null)
                return;

            var declaredElement = declaration.DeclaredElement;
            if (declaredElement == null || declaredElement.ShortName == SharedImplUtil.MISSING_DECLARATION_NAME)
                return;

            IUnitTestElement testElement = null;
            var testClass = declaredElement as IClass;
            if (testClass != null)
                testElement = ProcessTestClass(testClass);

            var subElements = new List<IUnitTestElement>();

            var testMethod = declaredElement as IMethod;
            if (testMethod != null)
                testElement = ProcessTestMethod(testMethod, subElements);

            if (testElement != null)
            {
                // Ensure that the method has been implemented, i.e. it has a name and a document
                var nameRange = declaration.GetNameDocumentRange().TextRange;
                var documentRange = declaration.GetDocumentRange().TextRange;
                if (nameRange.IsValid && documentRange.IsValid)
                {
                    var disposition = new UnitTestElementDisposition(testElement, file.GetSourceFile().ToProjectFile(),
                                                                     nameRange, documentRange, subElements);
                    observer.OnUnitTestElementDisposition(disposition);
                }
            }
        }

        public void ProcessAfterInterior(ITreeNode element)
        {
        }

        private IUnitTestElement ProcessTestClass(IClass testClass)
        {
            if (IsAbstractClass(testClass))
                return ProcessAbstractTestClass(testClass);

            var typeInfo = testClass.AsTypeInfo();
            if (!IsValidTestClass(testClass))
                return null;

            XunitTestClassElement testElement;

            if (!classes.TryGetValue(testClass, out testElement))
            {
                var clrTypeName = testClass.GetClrName();
                testElement = unitTestElementFactory.GetOrCreateTestClass(project, clrTypeName, assemblyPath, typeInfo.GetTraits());

                classes.Add(testClass, testElement);
            }

            if (testElement != null)
            {
                foreach (var testMethod in IsInThisFile(testElement.Children))
                    testMethod.State = UnitTestElementState.Pending;

                // TODO: I think this might be an edge case with RunWith
                // We'll add Fact based methods for classes with RunWith + Facts in base classes
                AppendTests(testElement, typeInfo, testClass.GetAllSuperTypes());
            }

            return testElement;
        }

        private class InheritorsConsumer : IFindResultConsumer<ITypeElement>
        {
            private const int MaxInheritors = 50;

            private readonly HashSet<ITypeElement> elements = new HashSet<ITypeElement>();

            public IEnumerable<ITypeElement> FoundElements
            {
                get { return elements; }
            } 

            public ITypeElement Build(FindResult result)
            {
                var inheritedElement = result as FindResultInheritedElement;
                if (inheritedElement != null)
                    return (ITypeElement) inheritedElement.DeclaredElement;
                return null;
            }

            public FindExecution Merge(ITypeElement data)
            {
                elements.Add(data);
                return elements.Count < MaxInheritors ? FindExecution.Continue : FindExecution.Stop;
            }
        }

        private IUnitTestElement ProcessAbstractTestClass(IClass testClass)
        {
            var typeInfo = testClass.AsTypeInfo();
            if (!TypeUtility.ContainsTestMethods(typeInfo))
                return null;

            var solution = testClass.GetSolution();
            var inheritorsConsumer = new InheritorsConsumer();
            solution.GetPsiServices().Finder.FindInheritors(testClass, searchDomainFactory.CreateSearchDomain(solution, true),
                inheritorsConsumer, NullProgressIndicator.Instance);

            var elements = new List<XunitTestClassElement>();

            foreach (var element in inheritorsConsumer.FoundElements)
            {
                var declaration = element.GetDeclarations().FirstOrDefault();
                if (declaration != null)
                {
                    var elementProject = declaration.GetProject();
                    var elementAssemblyPath = assemblyPath;
                    if (!Equals(project, elementProject))
                    {
                        elementAssemblyPath = elementProject.GetOutputFilePath().FullPath;
                    }

                    var classElement = unitTestElementFactory.GetOrCreateTestClass(elementProject, element.GetClrName().GetPersistent(), elementAssemblyPath, typeInfo.GetTraits());
                    AppendTests(classElement, typeInfo, element.GetAllSuperTypes());

                    elements.Add(classElement);
                }
            }

            derivedTestClassElements[testClass] = elements;

            return null;
        }

        private void AppendTests(XunitTestClassElement classElement, ITypeInfo typeInfo, IEnumerable<IDeclaredType> types)
        {
            foreach (var declaredType in types)
            {
                if (declaredType.GetClrName().Equals(PredefinedType.OBJECT_FQN))
                    continue;

                var typeElement = declaredType.GetTypeElement();
                if (typeElement != null)
                {
                    var methodInfo = new PsiMethodInfoAdapter();
                    foreach (var method in typeElement.GetMembers().OfType<IMethod>())
                    {
                        methodInfo.Reset(method, typeInfo);
                        if (MethodUtility.IsTest(methodInfo))
                        {
                            unitTestElementFactory.GetOrCreateTestMethod(project, classElement, typeElement.GetClrName(),
                                                                         method.ShortName, MethodUtility.GetSkipReason(methodInfo),
                                                                         methodInfo.GetTraits(), false);
                        }
                    }
                }
            }
        }

        private IEnumerable<IUnitTestElement> IsInThisFile(IEnumerable<IUnitTestElement> unitTestElements)
        {
            return from element in unitTestElements
                   let projectFiles = element.GetProjectFiles()
                   where projectFiles == null || projectFiles.IsEmpty() || projectFiles.Contains(projectFile)
                   select element;
        }

        private static bool IsValidTestClass(IClass testClass)
        {
            return testClass.IsUnitTestContainer();
        }

        private static bool IsAbstractClass(IClass testClass)
        {
            // Static classes are abstract and sealed!
            return testClass.IsAbstract && !testClass.IsSealed;
        }

        private IUnitTestElement ProcessTestMethod(IMethod method, IList<IUnitTestElement> subElements)
        {
            var type = method.GetContainingType();
            var @class = type as IClass;
            if (type == null || @class == null)
                return null;

            var typeInfo = @class.AsTypeInfo();
            if (IsAbstractClass(@class) && TypeUtility.ContainsTestMethods(typeInfo))
                return ProcessTestMethodInAbstractClass(method, subElements);

            if (!IsValidTestClass(@class))
                return null;

            var command = TestClassCommandFactory.Make(typeInfo);
            if (command == null)
                return null;

            var testClassElement = classes[type];
            if (testClassElement == null)
                return null;

            var methodInfo = method.AsMethodInfo(typeInfo);
            if (command.IsTestMethod(methodInfo))
            {
                var clrTypeName = type.GetClrName();
                return unitTestElementFactory.GetOrCreateTestMethod(project, testClassElement, clrTypeName, method.ShortName, 
                    MethodUtility.GetSkipReason(methodInfo), methodInfo.GetTraits(), false);
            }

            return null;
        }

        private IUnitTestElement ProcessTestMethodInAbstractClass(IMethod method, IList<IUnitTestElement> subElements)
        {
            var containingType = method.GetContainingType();
            if (containingType == null)
                return null;

            IList<XunitTestClassElement> derivedElements;
            if (!derivedTestClassElements.TryGetValue(containingType, out derivedElements))
                return null;

            var inheritedTestMethodContainerElement = unitTestElementFactory.GetOrCreateInheritedTestMethodContainer(project,
                containingType.GetClrName(), method.ShortName);

            foreach (var derivedClassElement in derivedElements)
            {
                XunitTestMethodElement methodInDerivedClass = null;
                foreach (var testMethodElement in derivedClassElement.Children.OfType<XunitTestMethodElement>())
                {
                    if (testMethodElement.Id.Equals(inheritedTestMethodContainerElement.Id))
                    {
                        testMethodElement.State = UnitTestElementState.Valid;
                        methodInDerivedClass = testMethodElement;
                        break;
                    }
                }

                if (methodInDerivedClass == null)
                {
                    // TODO: Add traits
                    methodInDerivedClass = unitTestElementFactory.GetOrCreateTestMethod(project, derivedClassElement,
                        containingType.GetClrName().GetPersistent(), method.ShortName, string.Empty,
                        new OneToSetMap<string, string>(), false);
                }

                subElements.Add(methodInDerivedClass);
            }

            return inheritedTestMethodContainerElement;
        }
    }
}
