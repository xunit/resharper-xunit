using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.Elements;
using JetBrains.Util;
using Xunit.Sdk;
using XunitContrib.Runner.ReSharper.RemoteRunner;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    [SolutionComponent]
    public class UnitTestElementFactory
    {
        private readonly XunitTestProvider provider;
        private readonly UnitTestElementManager unitTestManager;
        private readonly DeclaredElementProvider declaredElementProvider;
        private readonly IUnitTestCategoryFactory categoryFactory;

        public UnitTestElementFactory(XunitTestProvider provider, UnitTestElementManager unitTestManager, DeclaredElementProvider declaredElementProvider, IUnitTestCategoryFactory categoryFactory)
        {
            this.provider = provider;
            this.unitTestManager = unitTestManager;
            this.declaredElementProvider = declaredElementProvider;
            this.categoryFactory = categoryFactory;
        }

        public XunitTestClassElement GetOrCreateTestClass(IProject project, IClrTypeName typeName, string assemblyLocation, MultiValueDictionary<string, string> traits)
        {
            var categories = GetCategories(traits);

            var id = new UnitTestElementId(provider, new PersistentProjectId(project), typeName.FullName);
            var element = unitTestManager.GetElementById(id);
            if (element != null)
            {
                element.State = UnitTestElementState.Valid;
                var classElement = element as XunitTestClassElement;
                if (classElement != null)   // Shouldn't be null, unless someone else has the same id
                {
                    classElement.AssemblyLocation = assemblyLocation;   // In case it's changed, e.g. someone's switched from Debug to Release
                    classElement.SetCategories(categories);
                }
                return classElement;
            }

            return new XunitTestClassElement(provider, new ProjectModelElementEnvoy(project), declaredElementProvider, typeName.FullName, typeName.GetPersistent(), assemblyLocation, categories);
        }

        public XunitTestMethodElement GetOrCreateTestMethod(IProject project, XunitTestClassElement testClassElement, IClrTypeName typeName,
                                                            string methodName, string skipReason, MultiValueDictionary<string, string> traits, bool isDynamic)
        {
            var categories = GetCategories(traits);

            var element = GetTestMethod(project, testClassElement, typeName, methodName);
            if (element != null)
            {
                element.State = UnitTestElementState.Valid;
                element.SetCategories(categories);
                return element;
            }
            return CreateTestMethod(provider, project, declaredElementProvider, testClassElement, typeName, methodName, skipReason, categories, isDynamic);
        }

        public XunitTestMethodElement GetTestMethod(IProject project, XunitTestClassElement classElement, IClrTypeName typeName, string methodName)
        {
            var methodId = GetTestMethodId(classElement, typeName, methodName);
            var id = new UnitTestElementId(provider, new PersistentProjectId(project), methodId);
            var unitTestElementManager = project.GetSolution().GetComponent<IUnitTestElementManager>();
            return unitTestElementManager.GetElementById(id) as XunitTestMethodElement;
        }

        public static XunitTestMethodElement CreateTestMethod(IUnitTestProvider provider, IProject project,
                                                              DeclaredElementProvider declaredElementProvider,
                                                              XunitTestClassElement classElement,
                                                              IClrTypeName typeName, string methodName,
                                                              string skipReason,
                                                              IEnumerable<UnitTestElementCategory> categories,
                                                              bool isDynamic = false)
        {
            var id = GetTestMethodId(classElement, typeName, methodName);
            return new XunitTestMethodElement(provider, classElement, new ProjectModelElementEnvoy(project),
                                              declaredElementProvider, id, typeName.GetPersistent(), methodName,
                                              skipReason, categories, isDynamic);
        }

        private static string GetTestMethodId(XunitTestClassElement classElement, IClrTypeName typeName, string methodName)
        {
            var baseTypeName = string.Empty;
            if (!classElement.TypeName.Equals(typeName))
                baseTypeName = typeName.ShortName + ".";

            return string.Format("{0}.{1}{2}", classElement.Id, baseTypeName, methodName);
        }

        private IEnumerable<UnitTestElementCategory> GetCategories(MultiValueDictionary<string, string> traits)
        {
            var categories = from key in traits
                             where !key.IsNullOrEmpty() && !key.IsWhitespace()
                             from value in traits[key]
                             where !value.IsNullOrEmpty() && !value.IsWhitespace()
                             select string.Compare(key, "category", StringComparison.InvariantCultureIgnoreCase) != 0
                                  ? string.Format("{0}[{1}]", key.Trim(), value.Trim())
                                  : value;
            return categoryFactory.Create(categories);
        }

        public XunitInheritedTestMethodContainerElement GetOrCreateInheritedTestMethodContainer(IProject project, IClrTypeName typeName, string methodName)
        {
            // See the comment in GetOrCreateTestClass re: dotCover showing ids instead of names.
            // This element never becomes a genuine test element, so dotCover will never see it,
            // but keep the id format the same
            var fullMethodName = typeName.FullName + "." + methodName;
            var id = new UnitTestElementId(provider, new PersistentProjectId(project), fullMethodName);
            var element = unitTestManager.GetElementById(id);
            if (element != null)
                return element as XunitInheritedTestMethodContainerElement;

            return new XunitInheritedTestMethodContainerElement(provider, new ProjectModelElementEnvoy(project), fullMethodName, typeName.GetPersistent(), methodName);
        }

        public XunitTestTheoryElement GetTestTheory(IProject project, XunitTestMethodElement methodElement, string name)
        {
            var theoryId = GetTestTheoryId(methodElement, GetTestTheoryShortName(name, methodElement));
            var id = new UnitTestElementId(provider, new PersistentProjectId(project), theoryId);
            var unitTestElementManager = project.GetSolution().GetComponent<IUnitTestElementManager>();
            return unitTestElementManager.GetElementById(id) as XunitTestTheoryElement;
        }

        public static XunitTestTheoryElement CreateTestTheory(IUnitTestProvider provider, IProject project, XunitTestMethodElement methodElement, string name)
        {
            var shortName = GetTestTheoryShortName(name, methodElement);
            var id = GetTestTheoryId(methodElement, shortName);
            return new XunitTestTheoryElement(provider, methodElement, new ProjectModelElementEnvoy(project), id, shortName, methodElement.Categories);
        }

        public XunitTestTheoryElement GetOrCreateTestTheory(IProject project, XunitTestMethodElement methodElement, string name)
        {
            var element = GetTestTheory(project, methodElement, name);
            return element ?? CreateTestTheory(provider, project, methodElement, name);
        }

        private static string GetTestTheoryShortName(string theoryName, XunitTestMethodElement methodElement)
        {
            var prefix = methodElement.TypeName.FullName + ".";
            var name = theoryName.StartsWith(prefix) ? theoryName.Substring(prefix.Length) : theoryName;
            return DisplayNameUtil.Escape(name);
        }

        private static string GetTestTheoryId(XunitTestMethodElement methodElement, string shortName)
        {
            return string.Format("{0}.{1}", methodElement.Id, shortName);
        }
    }
}