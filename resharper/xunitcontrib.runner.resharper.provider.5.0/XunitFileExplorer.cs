using System;
using System.Collections.Generic;
using JetBrains.Application;
using JetBrains.Application.Progress;
using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.Util;
using JetBrains.Util.DataStructures;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
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
            foreach (var type in types)
            {
                var typeElement = type.GetTypeElement();
                if (typeElement == null)
                    continue;

                var @class = typeElement as IClass;
                if (@class == null)
                    continue;

                var command = TestClassCommandFactory.Make(@class.AsTypeInfo());
                if (command == null)
                    continue;

                foreach (var method in command.EnumerateTestMethods())
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

        public void ProcessAfterInterior(IElement element)
        {
        }

        public void ProcessBeforeInterior(IElement element)
        {
            var declaration = element as IDeclaration;

            if (declaration != null)
            {
                XunitTestElement testElement = null;
                var declaredElement = declaration.DeclaredElement;

                var testClass = declaredElement as IClass;
                if (testClass != null)
                {
                    var typeInfo = testClass.AsTypeInfo();

                    if (testClass.GetAccessRights() == AccessRights.PUBLIC &&
                        TypeUtility.IsTestClass(typeInfo) &&
                        !TypeUtility.HasRunWith(typeInfo))
                    {
                        testElement = ProcessTestClass(testClass);
                    }
                }

                var testMethod = declaredElement as IMethod;
                if (testMethod != null)
                    testElement = ProcessTestMethod(testMethod) ?? testElement;

                if (testElement != null)
                {
                    // Ensure that the method has been implemented, i.e. it has a name and a document
                    var nameRange = declaration.GetNameDocumentRange().TextRange;
                    var documentRange = declaration.GetDocumentRange();
                    if (nameRange.IsValid() && documentRange.IsValid())
                    {
                        var disposition = new UnitTestElementDisposition(testElement, file.ProjectFile,
                            nameRange, documentRange.TextRange);
                        consumer(disposition);
                    }
                }
            }
        }

        private XunitTestElement ProcessTestClass(ITypeElement type)
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

        private XunitTestElement ProcessTestMethod(IMethod method)
        {
            var type = method.GetContainingType();
            if (type == null)
                return null;

            var @class = type as IClass;
            if (@class == null)
                return null;

            var command = TestClassCommandFactory.Make(@class.AsTypeInfo());
            if (command == null)
                return null;

            var fixtureElementClass = classes[type];
            if (fixtureElementClass == null)
                return null;

            if (command.IsTestMethod(method.AsMethodInfo()))
            {
                var order = orders[type] + 1;
                orders[type] = order;
                return new XunitTestElementMethod(provider, fixtureElementClass, project, type.CLRName, method.ShortName, order);
            }

            return null;
        }
    }
}