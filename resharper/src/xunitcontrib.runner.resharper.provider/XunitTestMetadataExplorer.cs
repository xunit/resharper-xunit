using System.Collections.Generic;
using System.Linq;
using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.UnitTestFramework;
using Xunit.Sdk;
using XunitContrib.Runner.ReSharper.RemoteRunner;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    [MetadataUnitTestExplorer]
    public class XunitTestMetadataExplorer : IUnitTestMetadataExplorer
    {
        private readonly XunitTestProvider provider;
        private readonly UnitTestElementFactory unitTestElementFactory;

        public XunitTestMetadataExplorer(XunitTestProvider provider, UnitTestElementFactory unitTestElementFactory, UnitTestingAssemblyLoader assemblyLoader)
        {
            this.provider = provider;
            this.unitTestElementFactory = unitTestElementFactory;

            // Hmm. Not sure I like this here - needs to be here so that ReSharper will load
            // the runner assembly from the external process, so that assumes this was done
            assemblyLoader.RegisterAssembly(typeof(XunitTaskRunner).Assembly);
        }

        public void ExploreAssembly(IProject project, IMetadataAssembly assembly, UnitTestElementConsumer consumer)
        {
            foreach (var metadataTypeInfo in GetExportedTypes(assembly.GetTypes()))
            {
                ExploreType(project, assembly, consumer, metadataTypeInfo);
            }
        }

        public IUnitTestProvider Provider
        {
            get { return provider; }
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
            foreach (var type in (types ?? Enumerable.Empty<IMetadataTypeInfo>()).Where(IsPublic))
            {
                foreach (var nestedType in GetExportedTypes(type.GetNestedTypes()))
                {
                    yield return nestedType;
                }

                yield return type;
            }
        }

        private static bool IsPublic(IMetadataTypeInfo type)
        {
            // Hmmm. This seems a little odd. Resharper reports public nested types with IsNestedPublic,
            // while IsPublic is false
            return (type.IsNested && type.IsNestedPublic) || type.IsPublic;
        }

        private void ExploreType(IProject project, IMetadataAssembly assembly, UnitTestElementConsumer consumer, IMetadataTypeInfo metadataTypeInfo)
        {
            if (!metadataTypeInfo.IsUnitTestContainer()) return;

            var typeInfo = metadataTypeInfo.AsTypeInfo();
            var testClassCommand = TestClassCommandFactory.Make(typeInfo);
            if (testClassCommand == null)
                return;

            ExploreTestClass(project, assembly, consumer, metadataTypeInfo.FullyQualifiedName, testClassCommand.EnumerateTestMethods());
        }

        private void ExploreTestClass(IProject project, IMetadataAssembly assembly, UnitTestElementConsumer consumer, string typeName, IEnumerable<IMethodInfo> methods)
        {
            var classUnitTestElement = unitTestElementFactory.GetOrCreateTestClass(project, new ClrTypeName(typeName), assembly.Location.FullPath);
            consumer(classUnitTestElement);

            foreach (var methodInfo in methods.Where(MethodUtility.IsTest))
                ExploreTestMethod(project, classUnitTestElement, consumer, methodInfo);
        }

        private void ExploreTestMethod(IProject project, XunitTestClassElement classUnitTestElement, UnitTestElementConsumer consumer, IMethodInfo methodInfo)
        {
            var methodUnitTestElement = unitTestElementFactory.GetOrCreateTestMethod(project, classUnitTestElement, new ClrTypeName(methodInfo.TypeName), methodInfo.Name, MethodUtility.GetSkipReason(methodInfo));

            // TODO: Categories?
            consumer(methodUnitTestElement);
        }
    }
}