using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.Util;
using XunitContrib.Runner.ReSharper.RemoteRunner;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class UnitTestElementFactory
    {
        private readonly object lockObject = new object();
        private readonly XunitServiceProvider services;
        private readonly Action<IUnitTestElement> onUnitTestElementChanged;
        private readonly IDictionary<UnitTestElementId, IUnitTestElement> recentlyCreatedElements;

        public UnitTestElementFactory(XunitServiceProvider services, Action<IUnitTestElement> onUnitTestElementChanged, bool enableCache = true)
        {
            this.services = services;
            this.onUnitTestElementChanged = onUnitTestElementChanged;

            // Keep a track of elements that have been created, but not yet reported to the
            // IUnitTestElementManager (i.e. during parse)
            if (enableCache)
                recentlyCreatedElements = new Dictionary<UnitTestElementId, IUnitTestElement>();
        }

        private T GetElementById<T>(UnitTestElementId id) 
            where T : XunitBaseElement
        {
            IUnitTestElement value;
            if (recentlyCreatedElements != null && recentlyCreatedElements.TryGetValue(id, out value))
                return value as T;
            return services.ElementManager.GetElementById(id) as T;
        }

        private void CacheElement(UnitTestElementId id, IUnitTestElement element)
        {
            if (recentlyCreatedElements != null)
                recentlyCreatedElements[id] = element;
        }

        #region Classes

        public XunitTestClassElement GetOrCreateTestClass(PersistentProjectId projectId, IClrTypeName typeName,
                                                          string assemblyLocation,
                                                          OneToSetMap<string, string> traits)
        {
            var id = typeName.FullName;
            return GetOrCreateTestClass(id, projectId, typeName, assemblyLocation, traits);
        }

        public XunitTestClassElement GetOrCreateTestClass(string id, PersistentProjectId projectId,
                                                          IClrTypeName typeName,
                                                          string assemblyLocation,
                                                          OneToSetMap<string, string> traits)
        {
            lock (lockObject)
            {
                var elementId = services.CreateId(projectId, id);
                var element = GetElementById<XunitTestClassElement>(elementId);
                if (element == null)
                {
                    element = new XunitTestClassElement(services, elementId, typeName.GetPersistent(), assemblyLocation);
                    CacheElement(elementId, element);
                }

                element.State = UnitTestElementState.Valid;
                // Assembly location might have changed if someone e.g. switches from debug to release
                element.AssemblyLocation = assemblyLocation;

                services.ElementManager.RemoveElements(element.Children.Where(c => c.State == UnitTestElementState.Invalid).ToSet());

                UpdateCategories(element, traits);

                return element;
            }
        }

        #endregion


        #region Methods

        public XunitTestMethodElement GetOrCreateTestMethod(PersistentProjectId projectId,
                                                            XunitTestClassElement testClassElement,
                                                            IClrTypeName typeName, string methodName, string skipReason,
                                                            OneToSetMap<string, string> traits, bool isDynamic)
        {
            var baseTypeName = string.Empty;
            if (!testClassElement.TypeName.Equals(typeName))
                baseTypeName = typeName.ShortName + ".";

            var id = string.Format("{0}.{1}{2}", testClassElement.Id.Id, baseTypeName, methodName);

            return GetOrCreateTestMethod(id, projectId, testClassElement, typeName, methodName, skipReason, traits,
                isDynamic);
        }

        public XunitTestMethodElement GetOrCreateTestMethod(string id, PersistentProjectId projectId,
                                                            XunitTestClassElement testClassElement,
                                                            IClrTypeName typeName, string methodName,
                                                            string skipReason, OneToSetMap<string, string> traits,
                                                            bool isDynamic)
        {
            lock (lockObject)
            {
                var elementId = services.CreateId(projectId, id);
                var element = GetElementById<XunitTestMethodElement>(elementId);
                if (element == null)
                {
                    element = new XunitTestMethodElement(services, elementId, typeName, methodName, skipReason, isDynamic);
                    CacheElement(elementId, element);
                }

                element.Parent = testClassElement;
                element.State = UnitTestElementState.Valid;

                UpdateCategories(element, traits);

                return element;
            }
        }

        #endregion


        #region Theories

        public XunitTestTheoryElement GetOrCreateTestTheory(PersistentProjectId projectId, XunitTestMethodElement methodElement,
                                                            string name)
        {
            var id = string.Format("{0}.{1}", methodElement.Id.Id, GetTestTheoryShortName(name, methodElement));
            return GetOrCreateTestTheory(id, projectId, methodElement, name);
        }

        private static string GetTestTheoryShortName(string theoryName, XunitTestMethodElement methodElement)
        {
            // It's safe to pass in an existing shortname here
            var prefix = methodElement.TypeName.FullName + ".";
            var name = theoryName.StartsWith(prefix) ? theoryName.Substring(prefix.Length) : theoryName;
            return DisplayNameUtil.Escape(name);
        }

        public XunitTestTheoryElement GetOrCreateTestTheory(string id, PersistentProjectId projectId,
                                                            XunitTestMethodElement methodElement, string name)
        {
            lock (lockObject)
            {
                var elementId = services.CreateId(projectId, id);
                var element = GetElementById<XunitTestTheoryElement>(elementId);
                if (element == null)
                {
                    var shortName = GetTestTheoryShortName(name, methodElement);
                    element = new XunitTestTheoryElement(services, elementId, methodElement.TypeName, shortName);
                    CacheElement(elementId, element);
                }

                element.Parent = methodElement;
                element.State = UnitTestElementState.Valid;

                // Traits don't have their own categories, but can inherit from method and class
                UpdateCategories(element, new JetHashSet<string>());

                return element;
            }
        }

        #endregion

        public XunitInheritedTestMethodContainerElement GetOrCreateInheritedTestMethodContainer(PersistentProjectId projectId, IClrTypeName typeName, string methodName)
        {
            lock (lockObject)
            {
                var id = typeName.FullName + "." + methodName;
                var elementId = services.CreateId(projectId, id);

                var element = GetElementById<XunitInheritedTestMethodContainerElement>(elementId);
                if (element == null)
                {
                    element = new XunitInheritedTestMethodContainerElement(services, elementId, typeName.GetPersistent(),
                        methodName);
                    CacheElement(elementId, element);
                }

                return element;
            }
        }

        private void UpdateCategories(XunitBaseElement element, OneToSetMap<string, string> traits)
        {
            UpdateCategories(element, GetCategories(traits));
        }

        private void UpdateCategories(XunitBaseElement element, JetHashSet<string> newCategories)
        {
            using (UT.WriteLock())
            {
                lock (lockObject)
                {
                    var existingCategories = element.Categories
                        .Where(c => !ReferenceEquals(c, UnitTestElementCategory.UncategorizedCategory))
                        .ToHashSet(c => c.Name);
                    if (element.Parent != null &&
                        !ReferenceEquals(element.Parent.Categories, UnitTestElementCategory.Uncategorized))
                    {
                        newCategories.AddRange(element.Parent.Categories.Select(c => c.Name));
                    }

                    var removedCategories = existingCategories.Where(c => !newCategories.Contains(c)).ToList();

                    if (removedCategories.Any())
                        services.CategoryFactory.UnuseCategories(removedCategories);

                    if (newCategories.All(c => existingCategories.Contains(c)) && !removedCategories.Any())
                        return;

                    element.Categories = services.CategoryFactory.Create(newCategories);

                    // Notify ReSharper that the element has changed. We only need to do this for
                    // categories, and not private data, as ReSharper caches categories
                    if (onUnitTestElementChanged != null)
                        onUnitTestElementChanged(element);
                }
            }
        }

        private JetHashSet<string> GetCategories(OneToSetMap<string, string> traits)
        {
            var categories = from key in traits.Keys
                where !key.IsNullOrEmpty() && !key.IsWhitespace()
                from value in traits[key]
                where !value.IsNullOrEmpty() && !value.IsWhitespace()
                select string.Compare(key, "category", StringComparison.InvariantCultureIgnoreCase) != 0
                    ? string.Format("{0}[{1}]", key.Trim(), value.Trim())
                    : value;
            return categories.ToHashSet();
        }
    }
}