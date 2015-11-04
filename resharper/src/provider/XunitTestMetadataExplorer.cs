using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.ReSharper.UnitTestFramework;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class XunitTestMetadataExplorer
    {
        private readonly UnitTestElementFactory unitTestElementFactory;

        public XunitTestMetadataExplorer(UnitTestElementFactory unitTestElementFactory)
        {
            this.unitTestElementFactory = unitTestElementFactory;
        }

        public void ExploreAssembly(IProject project, IMetadataAssembly assembly, IUnitTestElementsObserver observer, CancellationToken cancellationToken)
        {
            using (ReadLockCookie.Create())
            {
                foreach (var metadataTypeInfo in GetExportedTypes(assembly.GetTypes()))
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    ExploreType(project, assembly, observer, metadataTypeInfo);
                }
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
            var projectId = new PersistentProjectId(project);
            var classUnitTestElement = unitTestElementFactory.GetOrCreateTestClass(projectId, new ClrTypeName(typeName), assembly.Location.FullPath, typeInfo.GetTraits());
            observer.OnUnitTestElement(classUnitTestElement);

            // Don't create elements for [Fact] methods when the class has [RunWith]. This
            // is because we don't know what the RunWith will do - it might not pay any
            // attention to [Fact], and if we create the elements now, they won't be
            // dynamic, and that can cause issues later
            if (!TypeUtility.HasRunWith(typeInfo))
            {
                foreach (var methodInfo in TypeUtility.GetTestMethods(typeInfo))
                    ExploreTestMethod(projectId, classUnitTestElement, observer, methodInfo);
            }
        }

        private void ExploreTestMethod(PersistentProjectId projectId, XunitTestClassElement classUnitTestElement, IUnitTestElementsObserver observer, IMethodInfo methodInfo)
        {
            var methodUnitTestElement = unitTestElementFactory.GetOrCreateTestMethod(projectId, classUnitTestElement, new ClrTypeName(methodInfo.TypeName), methodInfo.Name,
                MethodUtility.GetSkipReason(methodInfo), methodInfo.GetTraits(), false);
            observer.OnUnitTestElement(methodUnitTestElement);
        }
    }
}