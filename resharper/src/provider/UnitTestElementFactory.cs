using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.Elements;
using JetBrains.Util;
using XunitContrib.Runner.ReSharper.RemoteRunner;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    [SolutionComponent]
    public class UnitTestElementFactory
    {
        private readonly XunitTestProvider provider;
        private readonly IUnitTestElementManager unitTestManager;
        private readonly IUnitTestElementIdFactory unitTestElementIdFactory;
        private readonly DeclaredElementProvider declaredElementProvider;
        private readonly IUnitTestCategoryFactory categoryFactory;

        public UnitTestElementFactory(XunitTestProvider provider, IUnitTestElementManager unitTestManager, IUnitTestElementIdFactory unitTestElementIdFactory,
            DeclaredElementProvider declaredElementProvider, IUnitTestCategoryFactory categoryFactory)
        {
            this.provider = provider;
            this.unitTestManager = unitTestManager;
            this.unitTestElementIdFactory = unitTestElementIdFactory;
            this.declaredElementProvider = declaredElementProvider;
            this.categoryFactory = categoryFactory;
        }

        #region Classes

        public XunitTestClassElement GetOrCreateTestClass(IProject project, IClrTypeName typeName,
                                                          string assemblyLocation,
                                                          OneToSetMap<string, string> traits)
        {
            var id = unitTestElementIdFactory.Create(provider, new PersistentProjectId(project), typeName.FullName);
            return GetOrCreateTestClass(id, typeName, assemblyLocation, traits);
        }

        public XunitTestClassElement GetOrCreateTestClass(UnitTestElementId id, IClrTypeName typeName,
                                                          string assemblyLocation,
                                                          OneToSetMap<string, string> traits)
        {
            var categories = GetCategories(traits);

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

            return new XunitTestClassElement(id, declaredElementProvider, typeName.GetPersistent(), assemblyLocation, categories);
        }

        #endregion


        #region Methods

        public XunitTestMethodElement GetOrCreateTestMethod(IProject project, XunitTestClassElement testClassElement,
            IClrTypeName typeName, string methodName, string skipReason,
            OneToSetMap<string, string> traits, bool isDynamic)
        {
            var methodId = GetTestMethodId(testClassElement, typeName, methodName);
            var id = unitTestElementIdFactory.Create(provider, new PersistentProjectId(project), methodId);
            return GetOrCreateTestMethod(id, testClassElement, typeName, methodName, skipReason, traits, isDynamic);
        }

        public XunitTestMethodElement GetOrCreateTestMethod(UnitTestElementId id,
            XunitTestClassElement testClassElement, IClrTypeName typeName, string methodName, string skipReason,
            OneToSetMap<string, string> traits, bool isDynamic)
        {
            var categories = GetCategories(traits);

            var element = unitTestManager.GetElementById(id);
            if (element != null)
            {
                element.State = UnitTestElementState.Valid;
                element.Parent = testClassElement;

                var methodElement = element as XunitTestMethodElement;
                if (methodElement != null)
                    methodElement.SetCategories(categories);
                return methodElement;
            }

            return new XunitTestMethodElement(id, testClassElement, declaredElementProvider,
                typeName.GetPersistent(), methodName, skipReason, categories, isDynamic);
        }

        private static string GetTestMethodId(XunitTestClassElement classElement, IClrTypeName typeName, string methodName)
        {
            var baseTypeName = string.Empty;
            if (!classElement.TypeName.Equals(typeName))
                baseTypeName = typeName.ShortName + ".";

            return string.Format("{0}.{1}{2}", classElement.ShortId, baseTypeName, methodName);
        }

        #endregion


        #region Theories

        public XunitTestTheoryElement GetOrCreateTestTheory(IProject project, XunitTestMethodElement methodElement,
                                                            string name)
        {
            var theoryId = string.Format("{0}.{1}", methodElement.ShortId, GetTestTheoryShortName(name, methodElement));
            var id = unitTestElementIdFactory.Create(provider, new PersistentProjectId(project), theoryId);
            return GetOrCreateTestTheory(id, methodElement, name);
        }

        public XunitTestTheoryElement GetOrCreateTestTheory(UnitTestElementId id, XunitTestMethodElement methodElement,
                                                            string name)
        {
            var element = unitTestManager.GetElementById(id) as XunitTestTheoryElement;
            if (element != null)
            {
                element.State = UnitTestElementState.Valid;
                element.Parent = methodElement;

                return element;
            }

            var shortName = GetTestTheoryShortName(name, methodElement);
            return new XunitTestTheoryElement(id, methodElement, shortName, methodElement.Categories);
        }

        private static string GetTestTheoryShortName(string theoryName, XunitTestMethodElement methodElement)
        {
            // It's safe to pass in an existing shortname here
            var prefix = methodElement.TypeName.FullName + ".";
            var name = theoryName.StartsWith(prefix) ? theoryName.Substring(prefix.Length) : theoryName;
            return DisplayNameUtil.Escape(name);
        }

        #endregion

        private IEnumerable<UnitTestElementCategory> GetCategories(OneToSetMap<string, string> traits)
        {
            var categories = from key in traits.Keys
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
            var id = unitTestElementIdFactory.Create(provider, new PersistentProjectId(project), fullMethodName);
            var element = unitTestManager.GetElementById(id);
            if (element != null)
                return element as XunitInheritedTestMethodContainerElement;

            return new XunitInheritedTestMethodContainerElement(id, typeName.GetPersistent(), methodName);
        }
    }
}