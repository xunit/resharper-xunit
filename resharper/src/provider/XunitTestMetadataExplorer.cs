using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Application;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.ReSharper.UnitTestFramework;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public partial class XunitTestMetadataExplorer
    {
        private readonly XunitTestProvider provider;
        private readonly UnitTestElementFactory unitTestElementFactory;

        public XunitTestMetadataExplorer(XunitTestProvider provider, UnitTestElementFactory unitTestElementFactory)
        {
            this.provider = provider;
            this.unitTestElementFactory = unitTestElementFactory;
        }

#if !RESHARPER92
        public void ExploreAssembly(IProject project, IMetadataAssembly assembly, IUnitTestElementsObserver observer)
#else
        public void ExploreAssembly(IProject project, IMetadataAssembly assembly, IUnitTestElementsObserver observer, CancellationToken cancellationToken)
#endif
        {
            // TODO: Check cancellation token and exit early
            using (ReadLockCookie.Create())
            {
                foreach (var metadataTypeInfo in GetExportedTypes(assembly.GetTypes()))
                    ExploreType(project, assembly, observer, metadataTypeInfo);
            }
        }

        // TODO: Can we get rid of this now?

        // ReSharper's IMetadataAssembly.GetExportedTypes always seems to return an empty list, so
        // let's roll our own. MSDN says that Assembly.GetExportTypes is looking for "The only types
        // visible outside an assembly are public types and public types nested within other public types."
        // TODO: It might be nice to randomise this list:
        // However, this returns items in alphabetical ordering. Assembly.GetExportedTypes returns back in
        // the order in which classes are compiled (so the order in which their files appear in the msbuild file!)
        // with dependencies appearing first. 
        private static IEnumerable<IMetadataTypeInfo> GetExportedTypes(IEnumerable<IMetadataTypeInfo> types)
        {
            return (types ?? Enumerable.Empty<IMetadataTypeInfo>()).Where(IsPublic);
        }

        private static bool IsPublic(IMetadataTypeInfo type)
        {
            return (type.IsNested && type.IsNestedPublic) || type.IsPublic;
        }

        private void ExploreType(IProject project, IMetadataAssembly assembly, IUnitTestElementsObserver observer, IMetadataTypeInfo metadataTypeInfo)
        {
            // It would be nice to use TestClassCommandFactory.Make(...), but that doesn't work
            // with RunWith, since Make ends up calling TypeUtility.GetRunWith, which tries to
            // call IAttributeInfo.GetInstance<RunWithAttribute>, and we can't support that.
            // So we'll break down Make and do it ourselves. If the runner finds any methods
            // that we don't find, it will create them at runtime
            var typeInfo = metadataTypeInfo.AsTypeInfo();
            if (TypeUtility.IsTestClass(typeInfo))
                ExploreTestClass(project, assembly, observer, typeInfo, metadataTypeInfo.FullyQualifiedName);
        }

        private void ExploreTestClass(IProject project, IMetadataAssembly assembly, IUnitTestElementsObserver observer, ITypeInfo typeInfo, string typeName)
        {
            var classUnitTestElement = unitTestElementFactory.GetOrCreateTestClass(project, new ClrTypeName(typeName), assembly.Location.FullPath, typeInfo.GetTraits());

            // TODO: Should call OnUnitTestElement all the time, but only call OnUnitTestElementChanged if the element already exists and has just been updated
            // e.g. categories updated
            observer.OnUnitTestElement(classUnitTestElement);
            observer.OnUnitTestElementChanged(classUnitTestElement);

            // Don't create elements for [Fact] methods when the class has [RunWith]. This
            // is because we don't know what the RunWith will do - it might not pay any
            // attention to [Fact], and if we create the elements now, they won't be
            // dynamic, and that can cause issues later
            if (!TypeUtility.HasRunWith(typeInfo))
            {
                foreach (var methodInfo in TypeUtility.GetTestMethods(typeInfo))
                    ExploreTestMethod(project, classUnitTestElement, observer, methodInfo);
            }
        }

        private void ExploreTestMethod(IProject project, XunitTestClassElement classUnitTestElement, IUnitTestElementsObserver observer, IMethodInfo methodInfo)
        {
            var methodUnitTestElement = unitTestElementFactory.GetOrCreateTestMethod(project, classUnitTestElement, new ClrTypeName(methodInfo.TypeName), methodInfo.Name,
                MethodUtility.GetSkipReason(methodInfo), methodInfo.GetTraits(), false);
            observer.OnUnitTestElement(methodUnitTestElement);
            observer.OnUnitTestElementChanged(methodUnitTestElement);
        }

        // SDK9: TODO: When should I use OnUnitTestElementChanged?
    }
}