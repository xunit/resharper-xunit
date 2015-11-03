using System;
using System.Collections.Generic;
using System.Xml;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    using ReadFromXmlFunc = Func<XmlElement, IUnitTestElement, IProject, string, UnitTestElementFactory, IUnitTestElement>;

    [SolutionComponent]
    public class XunitTestElementSerializer : IUnitTestElementSerializer
    {
        private static readonly IDictionary<string, ReadFromXmlFunc> DeserialiseMap
            = new Dictionary<string, ReadFromXmlFunc>
            {
                {typeof (XunitTestClassElement).Name, XunitTestClassElement.ReadFromXml},
                {typeof (XunitTestMethodElement).Name, XunitTestMethodElement.ReadFromXml},
                {typeof (XunitTestTheoryElement).Name, XunitTestTheoryElement.ReadFromXml}
            };

        private readonly XunitServiceProvider services;

        public XunitTestElementSerializer(XunitServiceProvider services)
        {
            this.services = services;
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

        public IUnitTestElement DeserializeElement(XmlElement parent, string id, IUnitTestElement parentElement, IProject project)
        {
            if (!parent.HasAttribute("type"))
                throw new ArgumentException("Element is not xunit");

            ReadFromXmlFunc func;
            if (DeserialiseMap.TryGetValue(parent.GetAttribute("type"), out func))
            {
                var unitTestElementFactory = new UnitTestElementFactory(services, null);
                return func(parent, parentElement, project, id, unitTestElementFactory);
            }

            throw new ArgumentException("Element is not xunit");
        }

        public IUnitTestProvider Provider
        {
            get { return services.Provider; }
        }
    }
}
