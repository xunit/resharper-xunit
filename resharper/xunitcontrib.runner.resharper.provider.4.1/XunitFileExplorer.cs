using System;
using System.Collections.Generic;
using JetBrains.Application;
using JetBrains.Application.Progress;
using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.UnitTestExplorer;
using JetBrains.Util;
using JetBrains.Util.DataStructures;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper
{
    class XunitFileExplorer : IRecursiveElementProcessor
    {
        readonly string assemblyPath;
        readonly Dictionary2<ITypeElement, XunitTestElementClass> classes = new Dictionary2<ITypeElement, XunitTestElementClass>();
        readonly UnitTestElementLocationConsumer consumer;
        readonly IFile file;
        readonly CheckForInterrupt interrupted;
        readonly Dictionary2<IDeclaredElement, int> orders = new Dictionary2<IDeclaredElement, int>();
        readonly IProject project;
        readonly IUnitTestProvider provider;

        public XunitFileExplorer(IUnitTestProvider provider,
                                 UnitTestElementLocationConsumer consumer,
                                 IFile file,
                                 CheckForInterrupt interrupted)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            if (provider == null)
                throw new ArgumentNullException("provider");

            this.consumer = consumer;
            this.provider = provider;
            this.file = file;
            this.interrupted = interrupted;

            project = this.file.ProjectFile.GetProject();
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

        void AppendTests(XunitTestElementClass test,
                         IEnumerable<IDeclaredType> types,
                         ref int order)
        {
            foreach (IDeclaredType type in types)
            {
                ITypeElement typeElement = type.GetTypeElement();
                if (typeElement == null)
                    continue;

                IClass @class = typeElement as IClass;
                if (@class == null)
                    continue;

                ITestClassCommand command = TestClassCommandFactory.Make(TypeWrapper.Wrap(@class));
                if (command == null)
                    continue;

                foreach (IMethodInfo method in command.EnumerateTestMethods())
                    new XunitTestElementMethod(provider, test, project, typeElement.CLRName, method.Name, order++);

                AppendTests(test, type.GetSuperTypes(), ref order);
            }
        }

        public bool InteriorShouldBeProcessed(IElement element)
        {
            if (element is ITypeMemberDeclaration)
                return (element is ITypeDeclaration);

            return true;
        }

        public void ProcessAfterInterior(IElement element) {}

        public void ProcessBeforeInterior(IElement element)
        {
            IDeclaration declaration = element as IDeclaration;

            if (declaration != null)
            {
                XunitTestElement testElement = null;
                IDeclaredElement declaredElement = declaration.DeclaredElement;

                IClass testClass = declaredElement as IClass;
                if (testClass != null)
                {
                    ITypeInfo typeInfo = TypeWrapper.Wrap(testClass);

                    if (testClass.GetAccessRights() == AccessRights.PUBLIC &&
                        TypeUtility.IsTestClass(typeInfo) &&
                        !TypeUtility.HasRunWith(typeInfo))
                        testElement = ProcessTestClass(testClass);
                }

                IMethod testMethod = declaredElement as IMethod;
                if (testMethod != null)
                    testElement = ProcessTestMethod(testMethod) ?? testElement;

                if (testElement != null)
                {
                    // Ensure that the method has been implemented, i.e. it has a name and a document
                    TextRange nameRange = declaration.GetNameRange();
                    DocumentRange documentRange = declaration.GetDocumentRange();
                    if (nameRange.IsValid && documentRange.IsValid)
                    {
                        UnitTestElementDisposition disposition =
                            new UnitTestElementDisposition(testElement,
                                                           file.ProjectFile,
                                                           nameRange,
                                                           documentRange.TextRange);
                        consumer(disposition);
                    }
                }
            }
        }

        XunitTestElement ProcessTestClass(ITypeElement type)
        {
            XunitTestElementClass elementClass;

            if (classes.ContainsKey(type))
                elementClass = classes[type];
            else
            {
                elementClass = new XunitTestElementClass(provider, project, type.CLRName, assemblyPath);
                classes.Add(type, elementClass);
                orders.Add(type, 0);
            }

            XunitTestElement testElement = elementClass;
            //AppendTests(elementClass, type.GetSuperTypes(), ref order);
            return testElement;
        }

        XunitTestElement ProcessTestMethod(IMethod method)
        {
            ITypeElement type = method.GetContainingType();
            if (type == null)
                return null;

            IClass @class = type as IClass;
            if (@class == null)
                return null;

            ITestClassCommand command = TestClassCommandFactory.Make(TypeWrapper.Wrap(@class));
            if (command == null)
                return null;

            XunitTestElementClass fixtureElementClass = classes[type];
            if (fixtureElementClass == null)
                return null;

            if (command.IsTestMethod(MethodWrapper.Wrap(method)))
            {
                int order = orders[type] + 1;
                orders[type] = order;
                return new XunitTestElementMethod(provider, fixtureElementClass, project, type.CLRName, method.ShortName, order);
            }

            return null;
        }
    }
}