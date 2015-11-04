using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.Util;
using XunitContrib.Runner.ReSharper.RemoteRunner.Tasks;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class XunitTestClassElement : XunitBaseElement, ISerializableUnitTestElement, IEquatable<XunitTestClassElement>
    {
        public XunitTestClassElement(XunitServiceProvider services, UnitTestElementId id, IClrTypeName typeName,
                                     string assemblyLocation)
            : base(services, id, typeName)
        {
            AssemblyLocation = assemblyLocation;

            ShortName = string.Join("+", typeName.TypeNames.Select(FormatTypeName).ToArray());
        }

        private static string FormatTypeName(TypeNameAndTypeParameterNumber typeName)
        {
            return typeName.TypeName + (typeName.TypeParametersNumber > 0 ? string.Format("`{0}", typeName.TypeParametersNumber) : string.Empty);
        }

        public override string GetPresentation(IUnitTestElement parent, bool full)
        {
            return full ? Id.Id : ShortName;
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
            return Services.CachingService.GetTypeElement(Id.GetProject(), TypeName, true, true);
        }

        public override IEnumerable<IProjectFile> GetProjectFiles()
        {
            var declaredElement = GetDeclaredElement();
            if (declaredElement == null)
                return EmptyArray<IProjectFile>.Instance;

            return from sourceFile in declaredElement.GetSourceFiles()
                   select sourceFile.ToProjectFile();
        }

        public override IList<UnitTestTask> GetTaskSequence(ICollection<IUnitTestElement> explicitElements, IUnitTestRun run)
        {
            var knownMethods = from c in Children.OfType<XunitTestMethodElement>()
                select c.MethodName;
            var knownTheories = from c in Children.OfType<XunitTestMethodElement>()
                from gc in c.Children.OfType<XunitTestTheoryElement>()
                select gc.ShortName;
            var knownChildren = new HashSet<string>(knownMethods);
            knownChildren.AddRange(knownTheories);

            var projectId = Id.Project.Id;
            return new List<UnitTestTask>
                       {
                           new UnitTestTask(null, new XunitBootstrapTask(projectId)),
                           new UnitTestTask(null, new XunitTestAssemblyTask(projectId, AssemblyLocation)),
                           new UnitTestTask(this, new XunitTestClassTask(projectId, TypeName.FullName, explicitElements.Contains(this), knownChildren))
                       };
        }

        public override string Kind
        {
            get { return "xUnit.net Test Class"; }
        }

        public string AssemblyLocation { get; set; }

        public override bool Equals(IUnitTestElement other)
        {
            return Equals(other as XunitTestClassElement);
        }

        public bool Equals(XunitTestClassElement other)
        {
            if (other == null)
                return false;

            return Equals(Id, other.Id) &&
                   Equals(TypeName, other.TypeName) &&
                   Equals(AssemblyLocation, other.AssemblyLocation);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (XunitTestClassElement)) return false;
            return Equals((XunitTestClassElement) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = (TypeName != null ? TypeName.GetHashCode() : 0);
                result = (result*397) ^ (AssemblyLocation != null ? AssemblyLocation.GetHashCode() : 0);
                result = (result*397) ^ (Id.GetHashCode());
                return result;
            }
        }

        public void WriteToXml(XmlElement element)
        {
            element.SetAttribute("typeName", TypeName.FullName);
            element.SetAttribute("assemblyLocation", AssemblyLocation);
        }

        internal static IUnitTestElement ReadFromXml(XmlElement parent, IUnitTestElement parentElement,
            PersistentProjectId projectId, string id, UnitTestElementFactory elementFactory)
        {
            var typeName = parent.GetAttribute("typeName");
            var assemblyLocation = parent.GetAttribute("assemblyLocation");

            // TODO: Save and load traits. Might not be necessary - they are reset when scanning the file
            return elementFactory.GetOrCreateTestClass(id, projectId, new ClrTypeName(typeName), assemblyLocation,
                new OneToSetMap<string, string>());
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", GetType().Name, Id);
        }
    }
}