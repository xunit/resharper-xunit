using System;
using System.Collections.Generic;
using System.Xml;
using JetBrains.Application;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    using ReadFromXmlFunc = Func<XmlElement, IUnitTestElement, UnitTestElementId, UnitTestElementFactory, IUnitTestElement>;

    [SolutionComponent]
    public class XunitTestElementSerializer : IUnitTestElementSerializer
    {
        private static readonly IDictionary<string, ReadFromXmlFunc> DeserialiseMap = new Dictionary<string, ReadFromXmlFunc>
                                                                                          {
                                                                                              {typeof (XunitTestClassElement).Name, XunitTestClassElement.ReadFromXml},
                                                                                              {typeof (XunitTestMethodElement).Name, XunitTestMethodElement.ReadFromXml},
                                                                                              {typeof (XunitTestTheoryElement).Name, XunitTestTheoryElement.ReadFromXml}
                                                                                          };

        private readonly XunitTestProvider provider;
        private readonly IUnitTestElementIdFactory unitTestElementIdFactory;
        private readonly UnitTestElementFactory unitTestElementFactory;
        private readonly ISolution solution;
        private readonly IShellLocks locks;

        public XunitTestElementSerializer(XunitTestProvider provider, IUnitTestElementIdFactory unitTestElementIdFactory,
            UnitTestElementFactory unitTestElementFactory, ISolution solution, IShellLocks locks)
        {
            this.provider = provider;
            this.unitTestElementIdFactory = unitTestElementIdFactory;
            this.unitTestElementFactory = unitTestElementFactory;
            this.solution = solution;
            this.locks = locks;
        }

        public void SerializeElement(XmlElement parent, IUnitTestElement element)
        {
            parent.SetAttribute("type", element.GetType().Name);

            // Make sure that the element is actually ours before trying to serialise it
            // This can happen if there are two providers with the same "xunit" id installed
            var writableUnitTestElement = element as ISerializableUnitTestElement;
            if (writableUnitTestElement != null) 
                writableUnitTestElement.WriteToXml(parent);
        }

        // ReSharper 8.2
        public IUnitTestElement DeserializeElement(XmlElement parent, IUnitTestElement parentElement)
        {
            var id = parent.GetAttribute("Id");
            var projectId = parent.GetAttribute("ProjectId");

            IProject project;
            using (locks.UsingReadLock())
                project = ProjectUtil.FindProjectElementByPersistentID(solution, projectId) as IProject;

            return DeserializeElement(parent, id, parentElement, project, new PersistentProjectId(project));
        }

        // ReSharper 9.0
        public IUnitTestElement DeserializeElement(XmlElement parent, string id, IUnitTestElement parentElement, IProject project, PersistentProjectId projectId)
        {
            if (!parent.HasAttribute("type"))
                throw new ArgumentException("Element is not xunit");

            var unitTestElementId = unitTestElementIdFactory.Create(provider, projectId, id);

            ReadFromXmlFunc func;
            if (DeserialiseMap.TryGetValue(parent.GetAttribute("type"), out func))
                return func(parent, parentElement, unitTestElementId, unitTestElementFactory);

            throw new ArgumentException("Element is not xunit");
        }

        public IUnitTestProvider Provider
        {
            get { return provider; }
        }
    }
}