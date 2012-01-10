using System.Xml;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.UnitTestFramework;
using XunitContrib.Runner.ReSharper.RemoteRunner;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public partial class XunitTestProvider
    {
        private readonly IUnitTestElementSerializer unitTestElementSerializer;

        public XunitTestProvider(ISolution solution, CacheManager cacheManager, PsiModuleManager psiModuleManager, UnitTestElementManager unitTestElementManager)
        {
            Solution = solution;

            var factory = new UnitTestElementFactory(this, unitTestElementManager, cacheManager, psiModuleManager);
            unitTestElementSerializer = new XunitTestElementSerializer(this, factory, solution);

            var unitTestingAssemblyLoader = solution.GetComponent<UnitTestingAssemblyLoader>();
            unitTestingAssemblyLoader.RegisterAssembly(typeof (XunitTestRunner).Assembly);
        }

        public void SerializeElement(XmlElement parent, IUnitTestElement element)
        {
            unitTestElementSerializer.SerializeElement(parent, element);
        }

        public IUnitTestElement DeserializeElement(XmlElement parent, IUnitTestElement parentElement)
        {
            return unitTestElementSerializer.DeserializeElement(parent, parentElement);
        }

        public ISolution Solution { get; private set; }
    }
}