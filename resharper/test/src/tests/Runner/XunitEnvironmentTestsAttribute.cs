using System.Collections.Generic;
using JetBrains.Annotations;
using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.Runner
{
    public class XunitEnvironmentTestsAttribute : TestCaseSourceAttribute
    {
        public XunitEnvironmentTestsAttribute() : base(typeof(XunitEnvironmentTestsAttribute), "GetXunitEnvironments")
        {
        }

        [UsedImplicitly]
        public static IEnumerable<object> GetXunitEnvironments()
        {
            yield return new Xunit1Environment();
            yield return new Xunit2Environment();
        }
    }
}