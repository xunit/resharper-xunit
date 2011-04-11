using System;
using System.Collections.Generic;
using JetBrains.Application;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.UnitTestFramework;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    class XunitPsiFileExplorer : IRecursiveElementProcessor
    {
        private readonly IUnitTestProvider provider;
        private readonly UnitTestElementLocationConsumer consumer;
        private readonly IFile file;
        private readonly CheckForInterrupt interrupted;
        private readonly IProject project;
        private readonly string assemblyPath;

        private readonly Dictionary<ITypeElement, XunitViewTestClassElement> classes = new Dictionary<ITypeElement, XunitViewTestClassElement>();
        private readonly Dictionary<IDeclaredElement, int> orders = new Dictionary<IDeclaredElement, int>();

        public XunitPsiFileExplorer(IUnitTestProvider provider, UnitTestElementLocationConsumer consumer, IFile file, CheckForInterrupt interrupted)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            if (provider == null)
                throw new ArgumentNullException("provider");

            this.consumer = consumer;
            this.provider = provider;
            this.file = file;
            this.interrupted = interrupted;
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

        public void ProcessAfterInterior(ITreeNode element)
        {
        }

        public void ProcessBeforeInterior(ITreeNode element)
        {
            var declaration = element as IDeclaration;

            if (declaration != null)
            {
                IUnitTestViewElement testElement = null;
                var declaredElement = declaration.DeclaredElement;

                var testClass = declaredElement as IClass;
                if (testClass != null)
                    testElement = ProcessTestClass(testClass);

                var testMethod = declaredElement as IMethod;
                if (testMethod != null)
                    testElement = ProcessTestMethod(testMethod) ?? testElement;

                if (testElement != null)
                {
                    // Ensure that the method has been implemented, i.e. it has a name and a document
                    var nameRange = declaration.GetNameDocumentRange().TextRange;
                    var documentRange = declaration.GetDocumentRange();
                    if (nameRange.IsValid && documentRange.IsValid())
                    {
                        var disposition = new UnitTestElementDisposition(testElement, file.GetSourceFile().ToProjectFile(),
                            nameRange, documentRange.TextRange);
                        consumer(disposition);
                    }
                }
            }
        }

        private IUnitTestViewElement ProcessTestClass(IClass testClass)
        {
            if (!IsValidTestClass(testClass))
                return null;

            XunitViewTestClassElement testElement;

            if (!classes.TryGetValue(testClass, out testElement))
            {
                var clrTypeName = testClass.GetClrName();
                var typeName = clrTypeName.FullName;
                var shortName = clrTypeName.ShortName;
                testElement = new XunitViewTestClassElement(provider, project, typeName, shortName, assemblyPath);
                classes.Add(testClass, testElement);
                orders.Add(testClass, 0);
            }

            return testElement;
        }

        private static bool IsValidTestClass(IClass testClass)
        {
            return UnitTestElementIdentifier.IsUnitTestContainer(testClass) && !HasUnsupportedRunWith(testClass.AsTypeInfo());
        }

        private static bool HasUnsupportedRunWith(ITypeInfo typeInfo)
        {
            return TypeUtility.HasRunWith(typeInfo);
        }

        private IUnitTestViewElement ProcessTestMethod(IMethod method)
        {
            var type = method.GetContainingType();
            var @class = type as IClass;
            if (type == null || @class == null || !IsValidTestClass(@class))
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
                return new XunitViewTestMethodElement(provider, fixtureElementClass, project, type.GetClrName().FullName + "." + method.ShortName, type.GetClrName().FullName, method.ShortName, false);
            }

            return null;
        }
    }
}