using System;
using System.Xml.Serialization;
using JetBrains.UI.Updates;
using JetBrains.VSIntegration.Updates;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider.Updates
{
    [XmlType]
    [XmlRoot("PluginLocalInfo")]
    [Serializable]
    public class PluginLocalEnvironmentInfo
    {
        [XmlElement]
        public UpdateLocalEnvironmentInfoVs LocalEnvironment = new UpdateLocalEnvironmentInfoVs();

        [XmlElement]
        public UpdateLocalEnvironmentInfo.VersionSubInfo PluginVersion = new UpdateLocalEnvironmentInfo.VersionSubInfo();
    }
}