using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.Util;
using Xunit.Sdk;
using XunitContrib.Runner.ReSharper.RemoteRunner;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public partial class XunitTestMethodElement : XunitBaseElement, ISerializableUnitTestElement, IEquatable<XunitTestMethodElement>
    {
        private readonly string presentation;

        public XunitTestMethodElement(IUnitTestProvider provider, XunitTestClassElement testClass, ProjectModelElementEnvoy projectModelElementEnvoy,
                                      string id, IClrTypeName typeName, string methodName, string skipReason, IEnumerable<string> categories)
            : base(provider, testClass, id, projectModelElementEnvoy, categories)
        {
            TypeName = typeName;
            MethodName = methodName;
            ExplicitReason = skipReason;

            ShortName = MethodName;

            presentation = IsTestInParentClass() ? methodName : string.Format("{0}.{1}", TypeName.ShortName, MethodName);
        }

        private bool IsTestInParentClass()
        {
            return TestClass.TypeName.Equals(TypeName);
        }

        public IClrTypeName TypeName { get; private set; }
        public string MethodName { get; private set; }

        // ReSharper 7.0
        public override string GetPresentation(IUnitTestElement parentElement)
        {
            var inheritedTestMethodContainer = parentElement as XunitInheritedTestMethodContainerElement;
            if (inheritedTestMethodContainer == null)
                return presentation;

            if (String.Equals(inheritedTestMethodContainer.TypeName.FullName, TestClass.TypeName.FullName, StringComparison.InvariantCulture))
                return MethodName;

            return string.Format("{0}.{1}", Parent.GetPresentation(), MethodName);
        }

        public override UnitTestNamespace GetNamespace()
        {
            // Parent can be null for invalid elements
            return Parent != null ? Parent.GetNamespace() : new UnitTestNamespace(TypeName.GetNamespaceName());
        }

        public override UnitTestElementDisposition GetDisposition()
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

        public override IDeclaredElement GetDeclaredElement()
        {
            var declaredType = GetDeclaredType();
            if (declaredType == null)
                return null;

            // There is a small opportunity for this to choose the wrong method. If there is more than one
            // method with the same name (e.g. by error, or as an overload), this will arbitrarily choose the
            // first, whatever that means. Realistically, xunit throws an exception if there is more than
            // one method with the same name. We wouldn't know which one to go for anyway, unless we stored
            // the parameter types in this class. And that's overkill to fix such an edge case
            return (from member in declaredType.EnumerateMembers(MethodName, declaredType.CaseSensistiveName)
                    where member is IMethod
                    select member).FirstOrDefault();
        }

        private ITypeElement GetDeclaredType()
        {
            return Parent.GetDeclaredElement() as ITypeElement;
        }

        public override IEnumerable<IProjectFile> GetProjectFiles()
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

        public override IList<UnitTestTask> GetTaskSequence(ICollection<IUnitTestElement> explicitElements, IUnitTestLaunch launch)
        {
            var sequence = TestClass.GetTaskSequence(explicitElements, launch);
            sequence.Add(new UnitTestTask(this, new XunitTestMethodTask(Id, TestClass.AssemblyLocation, TypeName.FullName, ShortName, explicitElements.Contains(this))));
            return sequence;
        }

        private XunitTestClassElement TestClass
        {
            get { return Parent as XunitTestClassElement; }
        }

        public override string Kind
        {
            get { return "xUnit.net Test"; }
        }

        public override bool Equals(IUnitTestElement other)
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

            // TODO: Save and load traits. Not sure it's really necessary, the get updated when the file is scanned
            return unitTestElementFactory.GetOrCreateTestMethod(project, testClass, new ClrTypeName(typeName), methodName, skipReason, new MultiValueDictionary<string, string>());
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", GetType().Name, Id);
        }
    }
}