using System.Collections.Generic;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.Elements;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class UnitTestElementId
    {
        public UnitTestElementId(IUnitTestProvider provider, PersistentProjectId projectId, string id)
        {
            Provider = provider;
            PersistentProjectId = projectId;
            Id = id;
        }

        public string Id { get; private set; }
        public PersistentProjectId PersistentProjectId { get; private set; }
        public IUnitTestProvider Provider { get; private set; }

        public IProject GetProject()
        {
            return PersistentProjectId.Envoy.GetValidProjectElement() as IProject;
        }

        public override string ToString()
        {
            // TODO: Fix this up
            return PersistentProjectId + "::" + Id;
        }
    }

    public class PersistentProjectId
    {
        public PersistentProjectId(ProjectModelElementEnvoy envoy, string id)
        {
            Envoy = envoy;
            Id = id;
        }

        public PersistentProjectId(IProject project)
        {
            Envoy = new ProjectModelElementEnvoy(project);
            Id = project.GetPersistentID();
        }

        public ProjectModelElementEnvoy Envoy { get; private set; }
        public string Id { get; private set; }
    }

    public interface IUnitTestElementsObserver
    {
        void OnUnitTestElement(IUnitTestElement unitTestElement);
        void OnUnitTestElementChanged(IUnitTestElement unitTestElement);
        void OnUnitTestElementDisposition(UnitTestElementDisposition disposition);
    }

    public class UnitTestElementsObserver : IUnitTestElementsObserver
    {
        private readonly UnitTestElementConsumer consumer;
        private readonly UnitTestElementLocationConsumer locationConsumer;

        public UnitTestElementsObserver(UnitTestElementLocationConsumer locationConsumer)
        {
            this.locationConsumer = locationConsumer;
        }

        public UnitTestElementsObserver(UnitTestElementConsumer consumer)
        {
            this.consumer = consumer;
        }

        public void OnUnitTestElement(IUnitTestElement unitTestElement)
        {
            consumer(unitTestElement);
        }

        public void OnUnitTestElementChanged(IUnitTestElement unitTestElement)
        {
            consumer(unitTestElement);
        }

        public void OnUnitTestElementDisposition(UnitTestElementDisposition disposition)
        {
            locationConsumer(disposition);
        }
    }

    public interface IUnitTestCategoryFactory
    {
        IEnumerable<UnitTestElementCategory> Create(IEnumerable<string> categories);
    }

    public static class UnitTestElementManagerExtensions
    {
        public static IUnitTestElement GetElementById(this UnitTestElementManager manager, UnitTestElementId id)
        {
            return manager.GetElementById(id.GetProject(), id.Id);
        }
    }
}