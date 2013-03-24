using System.Drawing;
using JetBrains.ReSharper.TaskRunnerFramework;
using XunitContrib.Runner.ReSharper.RemoteRunner;
using XunitContrib.Runner.ReSharper.UnitTestProvider.Properties;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    partial class XunitTestProvider
    {
        public RemoteTaskRunnerInfo GetTaskRunnerInfo()
        {
            return new RemoteTaskRunnerInfo(typeof(XunitTaskRunner));
        }

        public Image Icon
        {
            get { return Resources.xunit; }
        }
    }
}
