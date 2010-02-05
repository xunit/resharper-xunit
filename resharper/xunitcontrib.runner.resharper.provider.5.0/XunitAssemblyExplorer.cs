using System.Collections.Generic;
using System.Linq;
using JetBrains.Application;
using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.UnitTestFramework;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    internal class XunitAssemblyExplorer : IMetadataTypeProcessor
    {
        private readonly IUnitTestProvider unitTestProvider;
        private readonly IMetadataAssembly assembly;
        private readonly UnitTestElementConsumer consumer;
        private readonly ProjectModelElementEnvoy projectEnvoy;

        internal XunitAssemblyExplorer(IUnitTestProvider unitTestProvider, IMetadataAssembly assembly, IProject project,
            UnitTestElementConsumer consumer)
        {
            this.unitTestProvider = unitTestProvider;
            this.assembly = assembly;
            this.consumer = consumer;

            // Copied from the nunit provider...
            using (ReadLockCookie.Create())
                projectEnvoy = new ProjectModelElementEnvoy(project);
        }

        public void ProcessTypeInfo(IMetadataTypeInfo metadataTypeInfo)
        {
            // Copied from the nunit provider...
            InterruptableActivityCookie.CheckAndThrow();

            var typeInfo = metadataTypeInfo.AsTypeInfo();
            if (TypeUtility.IsTestClass(typeInfo))  // TODO: What about HasRunWith support? Not supported in previous R# versions
            {
                var testClassCommand = TestClassCommandFactory.Make(typeInfo);
                if (testClassCommand == null)
                    return;

                ProcessTestClass(metadataTypeInfo.FullyQualifiedName, testClassCommand.EnumerateTestMethods());
            }
        }

        private void ProcessTestClass(string typeName, IEnumerable<IMethodInfo> methods)
        {
            var classUnitTestElement = new XunitTestElementClass(unitTestProvider, projectEnvoy, typeName, assembly.Location);
            consumer(classUnitTestElement);

            var order = 0;
            foreach (var method in methods.Where(MethodUtility.IsTest))
            {
                ProcessTestMethod(classUnitTestElement, method, order++);
            }
        }

        private void ProcessTestMethod(XunitTestElementClass classUnitTestElement, IMethodInfo method, int order)
        {
            var methodUnitTestElement = new XunitTestElementMethod(unitTestProvider,
                                                                   classUnitTestElement, projectEnvoy,
                                                                   method.TypeName, method.Name,
                                                                   order);
            methodUnitTestElement.SetExplicit(MethodUtility.GetSkipReason(method));
            // TODO: Categories?
            consumer(methodUnitTestElement);
        }
    }
}