using System;
using System.Collections.Generic;
using System.Xml;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    using ReadFromXmlFunc = Func<XmlElement, IUnitTestElement, ISolution, UnitTestElementFactory, IUnitTestElement>;

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
        private readonly UnitTestElementFactory unitTestElementFactory;
        private readonly ISolution solution;

        public XunitTestElementSerializer(XunitTestProvider provider, UnitTestElementFactory unitTestElementFactory, ISolution solution)
        {
            this.provider = provider;
            this.unitTestElementFactory = unitTestElementFactory;
            this.solution = solution;
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

        public IUnitTestElement DeserializeElement(XmlElement parent, string id, IUnitTestElement parentElement, IProject project, PersistentProjectId projectId)
        {
            if (!parent.HasAttribute("type"))
                throw new ArgumentException("Element is not xunit");

            ReadFromXmlFunc func;
            if (DeserialiseMap.TryGetValue(parent.GetAttribute("type"), out func))
                return func(parent, parentElement, solution, unitTestElementFactory);

            throw new ArgumentException("Element is not xunit");
        }

        public IUnitTestProvider Provider
        {
            get { return provider; }
        }
    }
}