using System;
using System.Collections.Generic;
using JetBrains.Application;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Search;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.Util;
using Xunit.Sdk;
using System.Linq;
using JetBrains.ReSharper.Psi.Util;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class XunitPsiFileExplorer : IRecursiveElementProcessor
    {
        private readonly UnitTestElementFactory unitTestElementFactory;
        private readonly UnitTestElementLocationConsumer consumer;
        private readonly IFile file;
        private readonly CheckForInterrupt interrupted;
        private readonly IProject project;
        private readonly string assemblyPath;
        private readonly IDictionary<ITypeElement, XunitTestClassElement> classes = new Dictionary<ITypeElement, XunitTestClassElement>();
        private readonly IDictionary<ITypeElement, IList<XunitTestClassElement>> derivedTestClassElements = new Dictionary<ITypeElement, IList<XunitTestClassElement>>(); 
        private readonly IProjectFile projectFile;

        // TODO: The nunit code uses UnitTestAttributeCache
        public XunitPsiFileExplorer(XunitTestProvider provider, UnitTestElementFactory unitTestElementFactory,
            UnitTestElementLocationConsumer consumer, IFile file, CheckForInterrupt interrupted)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            if (provider == null)
                throw new ArgumentNullException("provider");

            this.consumer = consumer;
            this.unitTestElementFactory = unitTestElementFactory;
            this.file = file;
            this.interrupted = interrupted;
            projectFile = file.GetSourceFile().ToProjectFile();
            project = file.GetProject();

            assemblyPath = UnitTestManager.GetOutputAssemblyPath(project).FullPath;
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

            if (declaration != null)
            {
                IUnitTestElement testElement = null;
                var declaredElement = declaration.DeclaredElement;

                var testClass = declaredElement as IClass;
                if (testClass != null)
                    testElement = ProcessTestClass(testClass);

                var subElements = new List<IUnitTestElement>();

                var testMethod = declaredElement as IMethod;
                if (testMethod != null)
                    testElement = ProcessTestMethod(testMethod, subElements) ?? testElement;

                if (testElement != null)
                {
                    // Ensure that the method has been implemented, i.e. it has a name and a document
                    var nameRange = declaration.GetNameDocumentRange().TextRange;
                    var documentRange = declaration.GetDocumentRange();
                    if (nameRange.IsValid && documentRange.IsValid())
                    {
                        var disposition = new UnitTestElementDisposition(testElement, file.GetSourceFile().ToProjectFile(),
                                                                         nameRange, documentRange.TextRange, subElements);
                        consumer(disposition);
                    }
                }
            }
        }

        public void ProcessAfterInterior(ITreeNode element)
        {
            // RS6.1 doesn't need this. Does 6.0?
            //var declaration = element as IDeclaration;

            //if (declaration != null)
            //{
            //    var declaredElement = declaration.DeclaredElement;

            //    var testClass = declaredElement as IClass;
            //    XunitTestClassElement testElement;
            //    if (testClass != null && classes.TryGetValue(testClass, out testElement))
            //    {
            //        foreach (var unitTestElement in testElement.Children.Where(x => x.State == UnitTestElementState.Pending))
            //        {
            //            unitTestElement.State = UnitTestElementState.Invalid;
            //        }
            //    }
            //}
        }

        private IUnitTestElement ProcessTestClass(IClass testClass)
        {
            if (testClass.IsAbstract)
                return ProcessAbstractTestClass(testClass);

            if (!IsValidTestClass(testClass))
                return null;

            XunitTestClassElement testElement;

            if (!classes.TryGetValue(testClass, out testElement))
            {
                var clrTypeName = testClass.GetClrName();
                testElement = unitTestElementFactory.GetOrCreateTestClass(project, clrTypeName, assemblyPath);

                classes.Add(testClass, testElement);
            }

            if (testElement != null)
            {
                foreach (var testMethod in IsInThisFile(testElement.Children))
                    testMethod.State = UnitTestElementState.Pending;

                AppendTests(testElement, testClass.GetAllSuperTypes());
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
            if (!TypeUtility.ContainsTestMethods(testClass.AsTypeInfo()))
                return null;

            var solution = testClass.GetSolution();
            var inheritorsConsumer = new InheritorsConsumer();
            solution.GetPsiServices().Finder.FindInheritors(testClass, SearchDomainFactory.Instance.CreateSearchDomain(solution, true),
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
                        elementAssemblyPath = UnitTestManager.GetOutputAssemblyPath(elementProject).FullPath;
                    }

                    var classElement = unitTestElementFactory.GetOrCreateTestClass(elementProject, element.GetClrName().GetPersistent(), elementAssemblyPath);
                    AppendTests(classElement, element.GetAllSuperTypes());

                    elements.Add(classElement);
                }
            }

            derivedTestClassElements[testClass] = elements;

            return null;
        }

        private void AppendTests(XunitTestClassElement classElement, IEnumerable<IDeclaredType> types)
        {
            foreach (var declaredType in types)
            {
                var typeElement = declaredType.GetTypeElement();
                if (typeElement != null)
                {
                    foreach (var method in typeElement.GetMembers().OfType<IMethod>())
                    {
                        var methodInfo = method.AsMethodInfo();
                        if (MethodUtility.IsTest(methodInfo))
                        {
                            unitTestElementFactory.GetOrCreateTestMethod(project, classElement, typeElement.GetClrName(),
                                                                         method.ShortName, MethodUtility.GetSkipReason(methodInfo));
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
            return testClass.IsUnitTestContainer() && !HasUnsupportedRunWith(testClass.AsTypeInfo());
        }

        private static bool HasUnsupportedRunWith(ITypeInfo typeInfo)
        {
            return TypeUtility.HasRunWith(typeInfo);
        }

        private IUnitTestElement ProcessTestMethod(IMethod method, IList<IUnitTestElement> subElements)
        {
            var type = method.GetContainingType();
            var @class = type as IClass;
            if (type == null || @class == null)
                return null;

            if (@class.IsAbstract && TypeUtility.ContainsTestMethods(@class.AsTypeInfo()))
                return ProcessTestMethodInAbstractClass(method, subElements);

            if (!IsValidTestClass(@class))
                return null;

            var command = TestClassCommandFactory.Make(@class.AsTypeInfo());
            if (command == null)
                return null;

            var testClassElement = classes[type];
            if (testClassElement == null)
                return null;

            var methodInfo = method.AsMethodInfo();
            if (command.IsTestMethod(methodInfo))
            {
                var clrTypeName = type.GetClrName();
                return unitTestElementFactory.GetOrCreateTestMethod(project, testClassElement, clrTypeName, method.ShortName, MethodUtility.GetSkipReason(methodInfo));
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
                    if (testMethodElement.Id == inheritedTestMethodContainerElement.Id)
                    {
                        testMethodElement.State = UnitTestElementState.Valid;
                        methodInDerivedClass = testMethodElement;
                        break;
                    }
                }

                if (methodInDerivedClass == null)
                {
                    methodInDerivedClass = unitTestElementFactory.GetOrCreateTestMethod(project, derivedClassElement,
                        containingType.GetClrName().GetPersistent(), method.ShortName, string.Empty);
                }

                subElements.Add(methodInDerivedClass);
            }

            return inheritedTestMethodContainerElement;
        }
    }
}
