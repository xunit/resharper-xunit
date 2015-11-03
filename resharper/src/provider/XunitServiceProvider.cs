using JetBrains.ProjectModel;
using JetBrains.ReSharper.TaskRunnerFramework;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.Elements;
using JetBrains.ReSharper.UnitTestFramework.Strategy;
using XunitContrib.Runner.ReSharper.RemoteRunner;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    [SolutionComponent]
    public class XunitServiceProvider
    {
        private readonly IUnitTestElementIdFactory elementIdFactory;
        private readonly IUnitTestRunStrategy runStrategy = new XunitOutOfProcessUnitTestRunStrategy();

        // ReSharper disable once SuggestBaseTypeForParameter
        public XunitServiceProvider(XunitTestProvider provider,
                                    IUnitTestElementManager elementManager,
                                    IUnitTestElementIdFactory elementIdFactory,
                                    IUnitTestElementCategoryFactory categoryFactory,
                                    UnitTestingCachingService cachingService)
        {
            this.elementIdFactory = elementIdFactory;
            CachingService = cachingService;
            ElementManager = elementManager;
            CategoryFactory = categoryFactory;
            Provider = provider;
        }

        public IUnitTestElementManager ElementManager { get; private set; }
        public IUnitTestElementCategoryFactory CategoryFactory { get; private set; }
        public IUnitTestProvider Provider { get; private set; }
        public IUnitTestRunStrategy RunStrategy { get { return runStrategy; } }
        public UnitTestingCachingService CachingService { get; private set; }

        public UnitTestElementId CreateId(IProject project, string id)
        {
            return elementIdFactory.Create(Provider, project, id);
        }
    }

    public class XunitOutOfProcessUnitTestRunStrategy : OutOfProcessUnitTestRunStrategy
    {
        public XunitOutOfProcessUnitTestRunStrategy()
            : base(new RemoteTaskRunnerInfo(XunitTaskRunner.RunnerId, typeof(XunitTaskRunner)))
        {
        }
    }
}