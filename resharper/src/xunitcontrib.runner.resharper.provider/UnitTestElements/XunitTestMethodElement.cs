using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.Util;
using XunitContrib.Runner.ReSharper.RemoteRunner;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public partial class XunitTestMethodElement : IUnitTestElement, ISerializableUnitTestElement, IEquatable<XunitTestMethodElement>
    {
        private readonly ProjectModelElementEnvoy projectModelElementEnvoy;
        private readonly CacheManager cacheManager;
        private readonly PsiModuleManager psiModuleManager;
        private readonly string presentation;
        private XunitTestClassElement parent;

        public XunitTestMethodElement(IUnitTestProvider provider, XunitTestClassElement testClass, ProjectModelElementEnvoy projectModelElementEnvoy, CacheManager cacheManager,
            PsiModuleManager psiModuleManager, string id, IClrTypeName typeName, string methodName, string skipReason)
        {
            Provider = provider;
            Parent = testClass;
            this.projectModelElementEnvoy = projectModelElementEnvoy;
            this.cacheManager = cacheManager;
            this.psiModuleManager = psiModuleManager;
            Id = id;
            TypeName = typeName;
            MethodName = methodName;
            ExplicitReason = skipReason;
            Children = new List<IUnitTestElement>();
            State = UnitTestElementState.Valid;

            presentation = IsTestInSameClass() ? methodName : string.Format("{0}.{1}", TypeName.ShortName, MethodName);
        }

        private bool IsTestInSameClass()
        {
            return parent.TypeName.Equals(TypeName);
        }

        public IClrTypeName TypeName { get; private set; }
        public string MethodName { get; private set; }

        public IProject GetProject()
        {
            return projectModelElementEnvoy.GetValidProjectElement() as IProject;
        }

        // ReSharper 6.1
        public string GetPresentation()
        {
            return presentation;
        }

        // ReSharper 7.0
        public string GetPresentation(IUnitTestElement parentElement)
        {
            var inheritedTestMethodContainer = parentElement as XunitInheritedTestMethodContainerElement;
            if (inheritedTestMethodContainer == null)
                return GetPresentation();

            if (Equals(inheritedTestMethodContainer.TypeName.FullName, parent.TypeName.FullName))
                return MethodName;

            return string.Format("{0}.{1}", parent.GetPresentation(), MethodName);
        }

        public UnitTestNamespace GetNamespace()
        {
            return parent.GetNamespace();
        }

        public UnitTestElementDisposition GetDisposition()
        {
            var element = GetDeclaredElement();
            if (element == null || !element.IsValid())
                return UnitTestElementDisposition.InvalidDisposition;

            var locations = from declaration in element.GetDeclarations()
                            let file = declaration.GetContainingFile()
                            where file != null
                            select new UnitTestElementLocation(file.GetSourceFile().ToProjectFile(),
                                                               declaration.GetNameDocumentRange().TextRange,
                                                               declaration.GetDocumentRange().TextRange);
            return new UnitTestElementDisposition(locations, this);
        }

        public IDeclaredElement GetDeclaredElement()
        {
            var declaredType = GetDeclaredType();
            if (declaredType == null)
                return null;

            return (from member in declaredType.EnumerateMembers(MethodName, declaredType.CaseSensistiveName)
                    let method = member as IMethod
                    where method != null && !method.IsAbstract && method.TypeParameters.Count <= 0 && (method.AccessibilityDomain.DomainType == AccessibilityDomain.AccessibilityDomainType.PUBLIC || method.AccessibilityDomain.DomainType == AccessibilityDomain.AccessibilityDomainType.INTERNAL)
                    select member).FirstOrDefault();
        }

        private ITypeElement GetDeclaredType()
        {
            var p = GetProject();
            if (p == null)
                return null;

            var psiModule = psiModuleManager.GetPrimaryPsiModule(p);
            if (psiModule == null)
                return null;

            return cacheManager.GetDeclarationsCache(psiModule, true, true).GetTypeElementByCLRName(TypeName);
        }

        public IEnumerable<IProjectFile> GetProjectFiles()
        {
            var declaredType = GetDeclaredType();
            if (declaredType != null)
            {
                var result = (from sourceFile in declaredType.GetSourceFiles() 
                              select sourceFile.ToProjectFile()).ToList<IProjectFile>();
                if (result.Count == 1)
                    return result;
            }

            var declaredElement = GetDeclaredElement();
            if (declaredElement == null)
                return EmptyArray<IProjectFile>.Instance;

            return from sourceFile in declaredElement.GetSourceFiles()
                   select sourceFile.ToProjectFile();
        }

        // ReSharper 6.1
        public IList<UnitTestTask> GetTaskSequence(IList<IUnitTestElement> explicitElements)
        {
            return GetTaskSequence(explicitElements, null);
        }

        // ReSharper 7.0
        public IList<UnitTestTask> GetTaskSequence(ICollection<IUnitTestElement> explicitElements, IUnitTestLaunch launch)
        {
            var sequence = TestClass.GetTaskSequence(explicitElements, launch);
            sequence.Add(new UnitTestTask(this, new XunitTestMethodTask(Id, TestClass.AssemblyLocation, TypeName.FullName, ShortName, explicitElements.Contains(this))));
            return sequence;
        }

        private XunitTestClassElement TestClass
        {
            get { return Parent as XunitTestClassElement; }
        }

        public string Kind
        {
            get { return "xUnit.net Test"; }
        }

        public IEnumerable<UnitTestElementCategory> Categories
        {
            get { return UnitTestElementCategory.Uncategorized; }
        }

        public string ExplicitReason { get; private set; }
        public string Id { get; private set; }
        public IUnitTestProvider Provider { get; private set; }

        public IUnitTestElement Parent
        {
            get { return parent; }
            set
            {
                if (parent == value)
                    return;

                if (parent != null)
                    parent.RemoveChild(this);
                parent = (XunitTestClassElement) value;
                if (parent != null)
                    parent.AddChild(this);
            }
        }

        public ICollection<IUnitTestElement> Children { get; private set; }

        public string ShortName
        {
            get { return MethodName; }
        }

        public bool Explicit
        {
            get { return !string.IsNullOrEmpty(ExplicitReason); }
        }

        public UnitTestElementState State { get; set; }

        public void AddChild(XunitTestTheoryElement xunitTestMethodElement)
        {
            Children.Add(xunitTestMethodElement);
        }

        public void RemoveChild(XunitTestTheoryElement xunitTestMethodElement)
        {
            Children.Remove(xunitTestMethodElement);
        }

        public bool Equals(IUnitTestElement other)
        {
            return Equals(other as XunitTestMethodElement);
        }

        public bool Equals(XunitTestMethodElement other)
        {
            if (other == null)
                return false;

            return Equals(Id, other.Id) &&
                   Equals(TypeName.FullName, other.TypeName.FullName) &&
                   Equals(MethodName, other.MethodName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (XunitTestMethodElement)) return false;
            return Equals((XunitTestMethodElement) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = (TypeName.FullName != null ? TypeName.FullName.GetHashCode() : 0);
                result = (result*397) ^ (Id != null ? Id.GetHashCode() : 0);
                result = (result*397) ^ (MethodName != null ? MethodName.GetHashCode() : 0);
                return result;
            }
        }

        public void WriteToXml(XmlElement element)
        {
            element.SetAttribute("projectId", GetProject().GetPersistentID());
            element.SetAttribute("typeName", TypeName.FullName);
            element.SetAttribute("methodName", MethodName);
            element.SetAttribute("skipReason", ExplicitReason);
        }

        internal static IUnitTestElement ReadFromXml(XmlElement parent, IUnitTestElement parentElement, ISolution solution, UnitTestElementFactory unitTestElementFactory)
        {
            var testClass = parentElement as XunitTestClassElement;
            if (testClass == null)
                throw new InvalidOperationException("parentElement should be xUnit.net test class");

            var typeName = parent.GetAttribute("typeName");
            var methodName = parent.GetAttribute("methodName");
            var projectId = parent.GetAttribute("projectId");
            var skipReason = parent.GetAttribute("skipReason");

            var project = (IProject)ProjectUtil.FindProjectElementByPersistentID(solution, projectId);
            if (project == null)
                return null;

            return unitTestElementFactory.GetOrCreateTestMethod(project, testClass, new ClrTypeName(typeName), methodName, skipReason);
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", GetType().Name, Id);
        }
    }
}