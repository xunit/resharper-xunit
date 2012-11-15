using System.Collections.Generic;
using System.Linq;
using Xunit.Extensions;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public static class MethodExtensions
    {
        public static IEnumerable<XunitTestTheoryTask> GetTheoryTasks(this Method method)
        {
            var parameters = method.Parameters.ToList();
            var customAttributes = method.GetCustomAttributes(typeof (InlineDataAttribute));

            foreach (var attribute in customAttributes)
            {
                var inlineDataAttribute = attribute.GetInstance<InlineDataAttribute>();
                if (inlineDataAttribute != null)
                {
                    var data = inlineDataAttribute.GetData(null, null).First();

                    var i = 0;
                    var formattedParameters = string.Join(", ", (from p in parameters
                                                                 select string.Format("{0}: {1}", p.Name, data[i++])).ToArray());
                    yield return new XunitTestTheoryTask(method.Task.ElementId, string.Format("{0}({1})", method.Name, formattedParameters));
                }
            }
        }
    }
}