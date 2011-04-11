using System.Collections.Generic;
using System.Linq;
using JetBrains.Metadata.Reader.API;
using JetBrains.ReSharper.TaskRunnerFramework.UnitTesting;
using Xunit.Sdk;
using XunitContrib.Runner.ReSharper.UnitTestRunnerProvider.TestElements;
using XunitContrib.Runner.ReSharper.UnitTestRunnerProvider.XunitSdkAdapters;

namespace XunitContrib.Runner.ReSharper.UnitTestRunnerProvider
{
    public class XunitRunnerMetadataExplorer
    {
        public XunitRunnerMetadataExplorer(XunitTestRunnerProvider provider)
        {
            UnitTestRunnerProvider = provider;
        }

        protected XunitTestRunnerProvider UnitTestRunnerProvider { get; set; }
        protected UnitTestElementConsumer Consumer { get; private set; }

        public void ExploreAssembly(IMetadataAssembly assembly, UnitTestElementConsumer consumer)
        {
            Consumer = consumer;
            foreach (var metadataTypeInfo in GetExportedTypes(assembly.GetTypes()))
            {
                ExploreType(assembly, metadataTypeInfo);
            }
        }

        private void ExploreType(IMetadataAssembly assembly, IMetadataTypeInfo metadataTypeInfo)
        {
            if (!UnitTestElementIdentifier.IsUnitTestContainer(metadataTypeInfo)) return;

            var typeInfo = metadataTypeInfo.AsTypeInfo();
            var testClassCommand = TestClassCommandFactory.Make(typeInfo);
            if (testClassCommand == null)
                return;

            ExploreTestClass(assembly, metadataTypeInfo.FullyQualifiedName, metadataTypeInfo.Name, testClassCommand.EnumerateTestMethods());
        }

        private void ExploreTestClass(IMetadataAssembly assembly, string typeName, string shortName, IEnumerable<IMethodInfo> methods)
        {
            var classUnitTestElement = GetOrCreateTestClassElement(assembly, typeName, shortName);
            Consumer(classUnitTestElement);

            foreach (var method in methods.Where(MethodUtility.IsTest))
            {
                ExploreTestMethod(classUnitTestElement, method);
            }
        }

        protected virtual XunitTestClassElement GetOrCreateTestClassElement(IMetadataAssembly assembly, string typeName, string shortName)
        {
            return new XunitTestClassElement(UnitTestRunnerProvider, typeName, shortName, assembly.Location.FullPath);
        }

        private void ExploreTestMethod(XunitTestClassElement classUnitTestElement, IMethodInfo method)
        {
            var methodUnitTestElement = GetOrCreateTestMethodElement(classUnitTestElement, 
                                                                method.TypeName, method.Name,
                                                                MethodUtility.IsSkip(method));
            // TODO: Categories?
            Consumer(methodUnitTestElement);
        }

        protected virtual XunitTestMethodElement GetOrCreateTestMethodElement(XunitTestClassElement parentTestClassElement, 
                                                                         string typeName, string methodName, bool isSkip)
        {
            return new XunitTestMethodElement(UnitTestRunnerProvider, parentTestClassElement,
                                              typeName + "." + methodName, typeName, methodName, isSkip);
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
    }
}